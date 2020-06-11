using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using WebApplication1.Models;
using WebApplication1.Security;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IDataProtector protector;
        public HomeController(IEmployeeRepository employeeRepository, IWebHostEnvironment hostingEnvironment, IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _employeeRepository = employeeRepository;
            _hostingEnvironment = hostingEnvironment;
            protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeStrings.EmployeeIdRouteValue);
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            //return Json(new { test = _employeeRepository.GetEmployee(1).Name });
            //return View();
            //return Json(new { yo="yooo",id=1337});
            var model = _employeeRepository.GetAllEmployees().Select(e => 
            {
                e.EncryptedId = protector.Protect(e.Id.ToString());
                return e;
            });
            return View(model);
        }
        [AllowAnonymous]
        public IActionResult Details(string id)
        {
            //return View();
            //Employee model = _employeeRepository.GetEmployee(1);
            //return Json(model);
            //return new ObjectResult(model);
            //return View();
            //ViewData["Employee"] = model;
            //ViewData["PageTitle"] = "Employee Details";
            //ViewBag.Employee = model;
            //ViewBag.PageTitle = "Employee Details";
            //return Json(new { Id = id, Name = name });
            int? employeeId = Convert.ToInt32(protector.Unprotect(id));
            HomeDetailsViewModel homeDetailsViewModel = new HomeDetailsViewModel();
            homeDetailsViewModel.Employee = _employeeRepository.GetEmployee(employeeId ?? 1);
            homeDetailsViewModel.Employee.EncryptedId = id;
            if (homeDetailsViewModel.Employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", employeeId);
            }
            homeDetailsViewModel.PageTitle = "Employee Details";
            return View(homeDetailsViewModel);
            
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model);
                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    PhotoPath = uniqueFileName
                };
                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }
            return View();
        }
        [HttpGet]
        public IActionResult Edit(string id)
        {
            int employeeId = Convert.ToInt32(protector.Unprotect(id));
            Employee employee = _employeeRepository.GetEmployee(employeeId);
            EmployeeEditViewModel employeeEditViewModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                ExistingPhotoPath = employee.PhotoPath
            };
            return View(employeeEditViewModel);
        }
        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                if (model.Photo != null)
                {
                    if(model.ExistingPhotoPath != null)
                    {
                        string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", model.ExistingPhotoPath);
                        System.IO.File.Delete(filePath);
                    }
                    employee.PhotoPath = ProcessUploadedFile(model);
                }
                _employeeRepository.Update(employee);
                return RedirectToAction("details", new { id = employee.Id });
            }
            return View();
        }
        [HttpPost]
        public IActionResult Delete(string id)
        {
            int employeeId = Convert.ToInt32(protector.Unprotect(id));
            _employeeRepository.Delete(employeeId);
            return RedirectToAction("Index");
        }
        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            if (model.Photo != null)
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
    }
}
