using PagedList;
using Shop.Areas.Admin.Models.ViewModels.Shop;
using Shop.Models.Data;
using Shop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        public ActionResult Categories()
        {
            //Объявляем модель типа List
            List<CategoryVM> categoryVMList;

            using(Db db = new Db())
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
            //Объявляем строковую переменную ID
            string id;
            using(Db db = new Db())
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

                //Сохраняем изменения
                db.Categories.Add(dto);
                db.SaveChanges();

                //Получаем ID, чтобы вернуть его в представление
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
                //Реализуем начальный счетчик
                int count = 1;

                //Инициализируем модель данных
                CategoryDTO dto;

                //Устанавливаем сортировку для каждой страницы
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        //Метод удаления страниц
        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Получаем модель категории
                CategoryDTO dto = db.Categories.Find(id);

                //Удаляем категорию
                db.Categories.Remove(dto);

                //Сохраняем изменения в базе
                db.SaveChanges();
            }
            //Добавляем сообщение об успешном удалении страницы
            TempData["SM"] = "You have deleted a category!";

            //Переадресация пользователя на метод Index
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

                // Получаем модель DTO(все данные из б/д)
                CategoryDTO dto = db.Categories.Find(id);

                // Редактируем модель DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                // Сохраняем изменения 
                db.SaveChanges();
            }
            // Возвращаем слово
            return "OK";
        }

        //Создаем метод добавления товаров
        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            // Объявляем модель данных
            ProductVM model = new ProductVM();

            // Добавляем список категорий из базы в модель
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), dataValueField: "id", dataTextField: "Name");
            }
            // Возвращаем модель в представление
            return View(model);
        }

        //Создаем метод добавления товаров
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
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            // Объявляем переменную ProductID
            int id;

            // Инициализируем и сохраняем в б/д модель на основе ProductDTO
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
            TempData["SM"] = "You have added a product!";

            #region Upload Image

            // Создаем необходимые ссылки на деректорий
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            // Проверяем наличие директорий (если нет, создаем)
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            // Проверяем, был ли файл загружен
            if (file != null && file.ContentLength > 0)
            {
                // Получаем расширение файла
                string ext = file.ContentType.ToLower();

                // Проверяем расширение файла
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }

                // Объявляем переменную с именем изображения
                string imageName = file.FileName;

                // Сохраняем имя изображения в модель DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                // Назначаем пути к оригинальному и уменьшенному изображению
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                // Сохраняем оригинальное изображение
                file.SaveAs(path);

                // Создаем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }
            #endregion

            // Переадресовываем пользователя
            return RedirectToAction("AddProduct");
        }

        //Создаем метод списка товаров
        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            // Объявляем модель ProductVM типа list
            List<ProductVM> listOfProductVM;

            // Установим номер страницы
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

        //Создаем метод редактирования товаров
        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // Объявляем модель ProductVM
            ProductVM model;

            using (Db db = new Db())
            {
                // Получаем товар
                ProductDTO dto = db.Products.Find(id);

                // Проверяем, доступен ли товар
                if (dto == null)
                {
                    return Content("That product does not exist.");
                }

                // Инициализируем модель данными
                model = new ProductVM(dto);

                // Создаем список категорий
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Получаем все изображения из галереи
                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }

            // Вернуть модель в представление
            return View(model);
        }

        //Создаем метод редактирования товаров
        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            // Получаем Id продукта
            int id = model.Id;

            // Заполняем список категориями и изображениями
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            // Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Проверяем имя товара на уникальность
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            // Обновляем товар в б/д
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
            TempData["SM"] = "You have edited the product";

            // Реализовываем логику обработки изображений
            #region Image Upload

            // Проверяем загрузку файла
            if (file != null && file.ContentLength > 0)
            {
                // Получаем расширение файла
                string ext = file.ContentType.ToLower();

                // Проверяем расширение
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }
                }

                // Устанавливаем пути загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                // Удаляем существующие файлы и директории
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                // Сохраняем имя изображения
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                // Сохраняем оригинал и превью версии
                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                // Сохраняем оригинальное изображение
                file.SaveAs(path);

                // Создаем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }

            #endregion

            // Переадресовываем пользователя
            return RedirectToAction("EditProduct");
        }

        //Создаем метод удаления товара
        // POST: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            // Удаляем товар из б/д
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            // Удаляем директории товара (изображения)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            // Переадресовываем пользователя
            return RedirectToAction("Products");
        }

        //Создаем метод добавления изображения в галерею
        // POST: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public void SaveGalleryImages(int id)
        {
            // Перебираем все полученные файлы
            foreach (string fileName in Request.Files)
            {
                // Инициализируем файлы
                HttpPostedFileBase file = Request.Files[fileName];

                // Проверяем на NULL
                if (file != null && file.ContentLength > 0)
                {
                    // Назначаем пути к директориям
                    var originalDirectoty = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectoty.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectoty.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    // Назначаем пути изображений
                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    // Сохраняем оригинальные изображения и их уменьшенные копии
                    file.SaveAs(path);

                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1, 1);
                    img.Save(path2);
                }
            }
        }

        //Создаем метод удаления изображений из галереи
        // POST: Admin/Shop/DeleteImage/id/imageName    
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1)) 
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        // Создаем метод вывода заказов администратора
        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            // Инициализируем модель OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                // Инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                // Перебираем данные модели OrderVM
                foreach (var order in orders)
                {
                    // Инициализируем словарь товаров
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    // Объявляем переменную общей суммы
                    decimal total = 0m;

                    // Инициализируем лист OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Получаем имя пользователя
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string username = user.Username;

                    // Перебираем список(лист) товаров из OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Получаем товар
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        // Получаем цену товара
                        decimal price = product.Price;

                        // Получаем название товара
                        string productName = product.Name;

                        // Добавляем товар в словарь
                        productAndQty.Add(productName, orderDetails.Quantity);

                        // Получаем полную стоимость всех товаров определенного пользователя
                        total += orderDetails.Quantity * price;
                    }

                    // Добавляем данные в модель OrdersForAdminVM
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
            // Возвращаем представление с моделью OrdersForAdminVM
            return View(ordersForAdmin);
        }
    }
}