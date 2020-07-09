using Shop.Models.Data;
using Shop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages     
        public ActionResult Index()
        {
            //Объявляем список для представления (PageVM)
            List<PageVM> pageList;

            //Инициализируем список (Db)
            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            //Возвращаем список в представление

            return View(pageList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {


                //Объявляем переменную для краткого описания (slug)
                string slug;

                //Инициализируем класс PageDTO
                PagesDTO dto = new PagesDTO();

                //Присваиваем заголовок модели 
                dto.Title = model.Title.ToUpper();

                //Проверяем, есть ли краткое описание. Если нет, то присваиваем его.
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Убеждаемся, что заголовок и краткое описание уникальные.
                if (db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist!");
                    return View(model);
                }
                else if (db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "That slug already exist!");
                    return View(model);
                }

                //Присваиваем оставшиеся значения модели
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                //Сохраняем модель в б/д
                db.Pages.Add(dto);
                db.SaveChanges();

            }

            //Передаем сообщение через TempData
            TempData["SM"] = "You have added a new page!";

            //Переадресовываем пользователя на метод INDEX
            return RedirectToAction("Index");
        }

        //Метод редактирования страниц
        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Объявляем модель PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Получаем данные страницы
                PagesDTO dto = db.Pages.Find(id);

                //Проверяем, доступна ли страница
                if (dto == null)
                {
                    return Content("The page does not exist!");
                }
                //Если доступна, то инициализируем модель данными
                model = new PageVM(dto);
            }

            //Возвращаем модель в представление
            return View(model);
        }

        //Метод редактирования страниц
        // POST: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Проверим модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //Получаем id страницы
                int id = model.Id;

                //Обьявим переменную краткого заголовка
                string slug = "home";

                //Получаем страницу по id
                PagesDTO dto = db.Pages.Find(id);

                //Присваиваем название из полученной модели в DTO
                dto.Title = model.Title;

                //Проверяем краткий заголовок и присваиваем его, если это необходимо
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //Проверяем краткий заголовок и заголовок на уникальность
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title already exist!");
                    return View(model);
                }
                else if (db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That slug already exist!");
                    return View(model);
                }

                //Записываем остальные значения в класс DTO
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Сохраняем изменения в базу
                db.SaveChanges();
            }

            //Оповещаем пользователя(сообщение) о сохранении в б/д в TempData
            TempData["SM"] = "You have edited the page.";

            //Переадресация пользователя
            return RedirectToAction("EditPage");
        }

        //Метод страницы деталей
        // GET: Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            //Объявляем модель PageVM
            PageVM model;

            using (Db db=new Db())
            {
                //Получаем страницу
                PagesDTO dto = db.Pages.Find(id);

                //Подтверждаем, что страница доступна
                if (dto == null)
                {
                    return Content("The page does not exist!");
                }

                //Присваиваем модели информацию из базы
                model = new PageVM(dto);
            }

            //Возвращаем модель в представления
            return View(model);
        }

        //Метод удаления страниц
        // GET: Admin/Pages/DeletePage/id
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //Получаем страницу
                PagesDTO dto = db.Pages.Find(id);

                //Удаляем страницу
                db.Pages.Remove(dto);

                //Сохраняем изменения в базе
                db.SaveChanges();
            }
            //Добавляем сообщение об успешном удалении страницы
            TempData["SM"] = "You have deleted a page!";

            //Переадресация пользователя на метод Index
            return RedirectToAction("Index");
        }

        //Создаем метод сортировки 
        // GET: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int [] id)
        {
            using(Db db = new Db())
            {
                //Реализуем начальный счетчик
                int count = 1;

                //Инициализируем модель данных
                PagesDTO dto;

                //Устанавливаем сортировку для каждой страницы
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }
        }

        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Объявляем(инициализируем) модель
            SidebarVM model;

            using (Db db = new Db())
            {
                //Получаем все данные из б/д
                SidebarDTO dto = db.Sidebars.Find(1); // согласен, не очень хорошая реализация 
                                                      //из-за "жесткого значения", но других идей не было

                //Заполняем модель данных,которую объявили
                model = new SidebarVM(dto);
            }
            //Вернуть представление с моделью
            return View(model);
        }

        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //Получаем данные из б/д
                SidebarDTO dto = db.Sidebars.Find(1);//Заменить!

                //Присваиваем данные в тело (в свойство Body)
                dto.Body = model.Body;

                //Сохраняем
                db.SaveChanges();
            }
            //Присваиваем сообщение в TempData об удачном исходе
            TempData["SM"] = "You have edited the sidebar!";

            //Переадресовываем пользователя
            return RedirectToAction("EditSidebar");
        }
    }
}