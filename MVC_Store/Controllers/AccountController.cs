using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Account;
using Microsoft.AspNet.Identity;
using MVC_Store.Models.ViewModels.Shop;

namespace MVC_Store.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST: account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // Проверяем модель на валидность
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            // Проверяем соответствие пароля
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password do not match!");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                // Проверяем имя на уникальность
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", $"Username {model.Username} is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                // Создаём экземпляр класса UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    Username = model.Username,
                    Password = model.Password
                };

                // Добавляем данные в модель
                db.Users.Add(userDTO);

                // Сохраняем данные
                db.SaveChanges();

                // Добавляем роль пользователю
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();
            }

            // Записать сообщение в TempData
            TempData["SM"] = "You are now registered and can login.";

            // Переадресовываем пользователя
            return RedirectToAction("Login");
        }

        // GET: Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            // подтвердить, что пользователь не авторизован
            string userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userName))
                return RedirectToAction("user-profile");

            // Возвращаем представление
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            // Проверяем модель на валидность
            if (!ModelState.IsValid)
                return View(model);

            // Проверяем пользователя на валидность
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                    isValid = true;

                if (!isValid)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }
        }

        // GET: /account/logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        
        [Authorize]
        public ActionResult UserNavPartial()
        {
            // Получаем имя пользователя
            string userName = User.Identity.Name;

            // Объявляем модель
            UserNavPartialVM model;

            using (Db db = new Db())
            { 
                // Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);


                // Заполняем модель данными из контекста (DTO)
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };
            }
            // Возвращаем частичное представление с моделью
            return PartialView(model);
        }

        // GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            // Получаем имя пользователя
            string userName = User.Identity.Name;

            // Объявляем модель
            UserProfileVM model;

            using (Db db = new Db())
            {
                // Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);

                // Инициализируем модель данными
                model = new UserProfileVM(dto);
            }
            // Возвращаем модель в представление
            return View("UserProfile", model);
        }

        // POST: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            bool userNameIsChanged = false;

            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Проверяем пароль (если пользователь его меняет)
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Получаем имя пользователя
                string userName = User.Identity.Name;

                // Проверяем, сменилось ли имя пользователя
                if (userName != model.Username)
                {
                    userName = model.Username;
                    userNameIsChanged = true;
                }

                // Проверяем имя на уникальность
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))
                {
                    ModelState.AddModelError("", $"Username {model.Username} alredy exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                // Изменяем модель контекста данных
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                // Сохраняем изменения
                db.SaveChanges();
            }

            // Устанавливаем сообщение в TempData
            TempData["SM"] = "You have edited your profile!";

            if (!userNameIsChanged)
                // Возвращаем представление с моделью
                return View("UserProfile", model);
            else
                return RedirectToAction("Logout");
        }

        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            //Инициализируем модель
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();
            using (Db db = new Db())
            {
                //Получаем ид пользователя
                UserDTO user = db.Users.FirstOrDefault(x => x.Username == User.Identity.Name);
                int userId = user.Id;
                //Инициализируем модель ордер
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();
                //Объявляем переменную конечной суммы
                foreach (var order in orders)
                {
                    //Инициализируем модель ордерс дитайлс дто
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();
                    //Объявляем переменную общей суммы
                    decimal total = 0m;
                    List<OrderDetailsDTO> orderDetailDto = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    //Перебираем список
                    foreach (var orderDetails in orderDetailDto)
                    {
                        //Получаем товар
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);
                        //Получаем цену
                        decimal price = product.Price;
                        //Получаем название товаров
                        string productname = product.Name;
                        //Добавляем товар в словарь
                        productAndQty.Add(productname, orderDetails.Quantity);
                        //Получаем общую стоимость товаров
                        total += orderDetails.Quantity * price;
                    }
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
                //Возвращаем представление с моделью
                return View(ordersForUser);
        }
    }
}