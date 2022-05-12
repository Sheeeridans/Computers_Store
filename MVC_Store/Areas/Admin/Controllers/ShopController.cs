using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using PagedList;
using MVC_Store.Areas.Admin.Models.ViewModels.Shop;

namespace MVC_Store.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //Объявляем модель типа List
            List<CategoryVM> categoryVMList;
            using (Db db = new Db())
            {
                //Инициализируем модель данными
                categoryVMList = db.Categories
                                 .ToArray()
                                 .OrderBy(x => x.Sorting)
                                 .Select(x => new CategoryVM(x))
                                 .ToList();
            }
                //Возвращаем List в представление
                return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            // Объявляем строковую переменную ID
            string id;
            using (Db db = new Db())
            {
                //Проверяем имя категории на уникальность
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                //Инициализируем модель DTO
                CategoryDTO dto = new CategoryDTO();

                //Добавляем данные в модель
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //Сохранить
                db.Categories.Add(dto);
                db.SaveChanges();

                //Получаем ID для возврата в представление
                id = dto.Id.ToString();

            }

            //Возвращаем ID в представление
            return id;
        }

        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Установить начальный счётчик
                int count = 1;
                //Объявить PageDTO
                CategoryDTO dto;
                //Установить сортировку для каждой страницы
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;
                }
            }
        }

        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Получаем модель категории
                CategoryDTO dto = db.Categories.Find(id);

                //Удаляем категорию
                db.Categories.Remove(dto);

                //Сохраняем изменения
                db.SaveChanges();
            }
            TempData["SM"] = "Вы удалили страницу";

            //Переадресовываем
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory/id
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                // Проверяем имя на уникальность
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";
                // Получаем модель DTO
                CategoryDTO dto = db.Categories.Find(id);
                // Редактируем модель DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();
                // Сохраняем изменения
                db.SaveChanges();
            }
            // Возвращаем слово
            return "ok";
        }

        //метод добавления товаров
        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Объявляем модель данных
            ProductVM model = new ProductVM();
            // Добавляем список категорий из базы в модель
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
            }
            // Возвращаем модель в представление
            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }
            // Проверяем имя продукта на уникальность
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Это имя занято");
                    return View(model);
                }
            }
            // Объявляем переменную ProductID
            int id;
            // Инициализируем и сохраняем модель на основе ProductDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;
                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;
                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }
            // Добавляем сообщение в TempData
            TempData["SM"] = "Вы добавили новый товар";

            // Переадресовываем пользователя
            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            // Объявляем ProductVM типа list
            List<ProductVM> listOfProductVM;
            // Устанавливаем номер страницы
            var pageNumber = page ?? 1;
            using (Db db = new Db())
            {
                // Инициализируем list и заполняем данными
                listOfProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();
                // Заполняем категории данными
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                // Устанавливаем выбранную категорию
                ViewBag.SelectedCat = catId.ToString();
            }
            // Устанавливаем постраничную навигацию
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;
            // Возвращаем представление с данными
            return View(listOfProductVM);
        }

        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // Объявляем модель ProductVM
            ProductVM model;
            using (Db db = new Db())
            {
                // Получаем продукт
                ProductDTO dto = db.Products.Find(id);
                // Проверяем, доступен ли продукт
                if (dto == null)
                {
                    return Content("Этот продукт недоступен");
                }
                // Инициализируем модель данными
                model = new ProductVM(dto);
                // Создаём список категорий
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
               }

            // Возвращаем модель в представление
            return View(model); 
        }

        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Получаем ID продукта
            int id = model.Id;
            // Заполняем список категориями и изображениями
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Проверяем имя продукта на уникальность
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            // Обновляем продукт
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;
                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;
                db.SaveChanges();
            }
            // Устанавливаем сообщение в TempData
            TempData["SM"] = "Вы изменили товар";

            // Переадресовываем пользователя
            return RedirectToAction("EditProduct");
        }

        // POST: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            // Удаляем товар из базы данных
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }
            //Сообщение о удалении страницы
            TempData["SM"] = "Вы удалили товар";
            // Переадресовываем пользователя
            return RedirectToAction("Products");
        }

        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            //Инициализируем модель ордер фор админ
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();
            using(Db db = new Db())
            {
                //Инициализируем модель ордер
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();
                //Перебираенм данные модели Ордер 
                foreach (var order in orders)
                {
                    //Инициализируем словарь данных
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();
                    //Объявляем переменную общей суммы
                    decimal total = 0m;
                    //Инициализируем ордел детаилс дио
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    //Получаем имя пользователя
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string username = user.Username;
                    //Перебираемсписок товаров изордер детайл
                    foreach (var orderDetails in orderDetailsList)
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
                    //добавляем данные в модель
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = username,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }
            //Возвращаем представление с моделью 
            return View(ordersForAdmin);
        }
    }
}