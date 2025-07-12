using System.Text;
using Diagnostic_Lab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Diagnostic_Lab.Controllers
{
    public class PatientController : Controller
    {
        private readonly HttpClient _client;

        public PatientController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7047/api/");
        }

        #region List Patients
        public async Task<IActionResult> PatientMenu()
        {
            try
            {
                var response = await _client.GetAsync("Patient");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to load patient data. Please try again.";
                    return View(new List<PatientModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var patients = JsonConvert.DeserializeObject<List<PatientModel>>(json);

                return View(patients ?? new List<PatientModel>());
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = "Unable to connect to the server. Please check your connection.";
                return View(new List<PatientModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return View(new List<PatientModel>());
            }
        }
        #endregion

        #region Add/Edit Form
        public async Task<IActionResult> AddEdit(int? PatientId)
        {
            try
            {
                // Prepare gender dropdown
                var genders = new List<string> { "Male", "Female", "Other" };
                ViewBag.Genders = new SelectList(genders);

                // Get users for dropdown (assuming you have an API endpoint to get users)
                var usersResponse = await _client.GetAsync("UserAccount");
                if (!usersResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to load user data.";
                    return RedirectToAction("PatientMenu");
                }

                var usersJson = await usersResponse.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserModel>>(usersJson) ?? new List<UserModel>();
                ViewBag.Users = new SelectList(users, "UserId", "FullName");

                PatientModel patient;

                if (PatientId == null)
                {
                    // New patient with defaults
                    patient = new PatientModel
                    {
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };
                }
                else
                {
                    // Edit existing patient
                    var response = await _client.GetAsync($"Patient/{PatientId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["ErrorMessage"] = "Patient not found.";
                        return RedirectToAction("PatientMenu");
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    patient = JsonConvert.DeserializeObject<PatientModel>(json) ?? new PatientModel();
                }

                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading patient form.";
                return RedirectToAction("PatientMenu");
            }
        }
        #endregion

        #region Save Patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEdit(PatientModel patient)
        {
            try
            {
                // Handle empty address
                if (string.IsNullOrWhiteSpace(patient.Address))
                {
                    patient.Address = null;
                }

                if (!ModelState.IsValid)
                {
                    // Repopulate dropdowns if validation fails
                    var genders = new List<string> { "Male", "Female", "Other" };
                    ViewBag.Genders = new SelectList(genders);

                    var usersResponse = await _client.GetAsync("UserAccount");
                    var usersJson = await usersResponse.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<List<UserModel>>(usersJson) ?? new List<UserModel>();
                    ViewBag.Users = new SelectList(users, "UserId", "FullName");

                    return View(patient);
                }

                // For new patients, set creation timestamp
                if (patient.PatientId == 0)
                {
                    patient.CreatedDate = DateTime.Now;
                }
                else
                {
                    patient.ModifiedDate = DateTime.Now;
                }

                var content = new StringContent(
                    JsonConvert.SerializeObject(patient),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response;
                if (patient.PatientId == 0)
                {
                    response = await _client.PostAsync("Patient", content);
                    TempData["SuccessMessage"] = "Patient added successfully!";
                }
                else
                {
                    response = await _client.PutAsync($"Patient/{patient.PatientId}", content);
                    TempData["SuccessMessage"] = "Patient updated successfully!";
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API returned status: {response.StatusCode}");
                }

                return RedirectToAction("PatientMenu");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to save patient: {ex.Message}";
                return RedirectToAction("PatientMenu");
            }
        }
        #endregion

        #region Delete Patient
        public async Task<IActionResult> Delete(int PatientId)
        {
            try
            {
                var response = await _client.DeleteAsync($"Patient/{PatientId}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Patient deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete patient.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting patient: {ex.Message}";
            }

            return RedirectToAction("PatientMenu");
        }
        #endregion
    }
}