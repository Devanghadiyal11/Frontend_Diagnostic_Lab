using System.Text;
using Diagnostic_Lab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Diagnostic_Lab.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _client;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7047/api/");
        }

        #region List Users
        public async Task<IActionResult> UserMenu()
        {
            try
            {
                var response = await _client.GetAsync("UserAccount");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to load user data. Please try again.";
                    return View(new List<UserModel>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<UserModel>>(json);

                return View(users ?? new List<UserModel>());
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = "Unable to connect to the server. Please check your connection.";
                return View(new List<UserModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return View(new List<UserModel>());
            }
        }
        #endregion

        #region Add/Edit Form
        public async Task<IActionResult> AddEdit(int? UserId)
        {
            try
            {
                // Prepare role dropdown
                var roles = new List<string> { "Doctor", "Technician", "Receptionist", "Patient" };
                ViewBag.Roles = new SelectList(roles);

                UserModel user;

                if (UserId == null)
                {
                    // New user with defaults
                    user = new UserModel
                    {
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                }
                else
                {
                    // Edit existing user
                    var response = await _client.GetAsync($"UserAccount/{UserId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["ErrorMessage"] = "User not found.";
                        return RedirectToAction("UserMenu");
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    user = JsonConvert.DeserializeObject<UserModel>(json) ?? new UserModel();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading user form.";
                return RedirectToAction("UserMenu");
            }
        }
        #endregion

        #region Save User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEdit(UserModel user)
        {
            try
            {
                // Handle empty mobile number
                if (string.IsNullOrWhiteSpace(user.MobileNo))
                {
                    user.MobileNo = null;
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = new SelectList(new List<string> { "Doctor", "Technician", "Receptionist", "Patient" });
                    return View(user);
                }

                // For new users, set creation timestamp
                if (user.UserId == 0)
                {
                    user.CreatedAt = DateTime.Now;
                }

                var content = new StringContent(
                    JsonConvert.SerializeObject(user),
                    Encoding.UTF8,
                    "application/json"
                );

                HttpResponseMessage response;
                if (user.UserId == 0)
                {
                    response = await _client.PostAsync("UserAccount", content);
                    TempData["SuccessMessage"] = "User added successfully!";
                }
                else
                {
                    response = await _client.PutAsync($"UserAccount/{user.UserId}", content);
                    TempData["SuccessMessage"] = "User updated successfully!";
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"API returned status: {response.StatusCode}");
                }

                return RedirectToAction("UserMenu");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to save user: {ex.Message}";
                return RedirectToAction("UserMenu");
            }
        }
        #endregion

        #region Delete User
        public async Task<IActionResult> Delete(int UserId)
        {
            try
            {
                var response = await _client.DeleteAsync($"UserAccount/{UserId}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction("UserMenu");
        }
        #endregion

    }
}