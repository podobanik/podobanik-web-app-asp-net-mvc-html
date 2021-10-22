﻿using WebAppAspNetMvcHtml.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace WebAppAspNetMvcHtml.Controllers
{
    public class ClientsController : Controller
    {
        private readonly string _key = "123456Qq";
        [HttpGet]
        public ActionResult Index()
        {
            GosuslugiContext db = new GosuslugiContext();
            var client = db.Clients.ToList();
            return View(client);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var client = new Client();
            return View(client);
        }

        [HttpPost]
        public ActionResult Create(Client model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var db = new GosuslugiContext();
            if (model.DocumentFile != null)
            {
                var data = new byte[model.DocumentFile.ContentLength];
                model.DocumentFile.InputStream.Read(data, 0, model.DocumentFile.ContentLength);

                model.Documents = new Document()
                {
                    Guid = Guid.NewGuid(),
                    DateChanged = DateTime.Now,
                    Data = data,
                    ContentType = model.DocumentFile.ContentType,
                    FileName = model.DocumentFile.FileName
                };
            }

            if (model.OrderIds != null && model.OrderIds.Any())
            {
                var orders = db.Orders.Where(s => model.OrderIds.Contains(s.Id)).ToList();
                model.Orders = orders;
            }
            if (model.AvailableDocumentIds != null && model.AvailableDocumentIds.Any())
            {
                var availableDocuments = db.AvailableDocuments.Where(s => model.AvailableDocumentIds.Contains(s.Id)).ToList();
                model.AvailableDocuments = availableDocuments;
            }
            db.Clients.Add(model);
            db.SaveChanges();
            return RedirectPermanent("/Clients/Index");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var db = new GosuslugiContext();
            var client = db.Clients.FirstOrDefault(x => x.Id == id);

            if (client == null)
                return RedirectPermanent("/Clients/Index");

            db.Clients.Remove(client);
            db.SaveChanges();

            return RedirectPermanent("/Clients/Index");

        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            var db = new GosuslugiContext();
            var client = db.Clients.FirstOrDefault(x => x.Id == id);

            if (client == null)
                return RedirectPermanent("/Clients/Index");


            return View(client);

        }

        [HttpPost]
        public ActionResult Edit(Client model)
        {


            var db = new GosuslugiContext();
            var client = db.Clients.FirstOrDefault(x => x.Id == model.Id);

            if (client == null)
                ModelState.AddModelError("Id", "Запись не найдена");
            if (model.Key != _key)
                ModelState.AddModelError("Key", "Ключ для создания/изменения записи указан не верно");

            if (!ModelState.IsValid)
                return View(model);

            MappingOrder(model, client, db);

            db.Entry(client).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectPermanent("/Clients/Index");
        }

        [HttpGet]
        public ActionResult GetImage(int id)
        {
            var db = new GosuslugiContext();
            var image = db.Documents.FirstOrDefault(x => x.Id == id);
            if (image == null)
            {
                FileStream fs = System.IO.File.OpenRead(Server.MapPath(@"~/Content/Images/not-foto.png"));
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                fs.Close();

                return File(new MemoryStream(fileData), "image/jpeg");
            }

            return File(new MemoryStream(image.Data), image.ContentType);
        }

        private void MappingOrder(Client source, Client destination, GosuslugiContext db)
        {
            destination.Name = source.Name;
            destination.Surname = source.Surname;
            destination.Age = source.Age;
            destination.Birthday = source.Birthday;
            destination.Gender = source.Gender;
            destination.ClientTypeId = source.ClientTypeId;
            destination.ClientTypes = source.ClientTypes;
            destination.IsArchive = source.IsArchive;
            destination.Reviews = source.Reviews;
            destination.Key = source.Key;


            if (destination.Orders != null)
                destination.Orders.Clear();

            if (source.OrderIds != null && source.OrderIds.Any())
                destination.Orders = db.Orders.Where(s => source.OrderIds.Contains(s.Id)).ToList();
            
            if (destination.Citizenships != null)
                destination.Citizenships.Clear();

            if (source.CitizenshipId != null && source.CitizenshipId.Any())
                destination.Citizenships = db.Citizenships.Where(s => source.CitizenshipId.Contains(s.Id)).ToList();

            if (destination.AvailableDocuments != null)
                destination.AvailableDocuments.Clear();

            if (source.AvailableDocumentIds != null && source.AvailableDocumentIds.Any())
                destination.AvailableDocuments = db.AvailableDocuments.Where(s => source.AvailableDocumentIds.Contains(s.Id)).ToList();

            if (source.DocumentFile != null)
            {
                var image = db.Documents.FirstOrDefault(x => x.Id == source.Id);


                var data = new byte[source.DocumentFile.ContentLength];
                source.DocumentFile.InputStream.Read(data, 0, source.DocumentFile.ContentLength);

                destination.Documents = new Document()
                {
                    Guid = Guid.NewGuid(),
                    DateChanged = DateTime.Now,
                    Data = data,
                    ContentType = source.DocumentFile.ContentType,
                    FileName = source.DocumentFile.FileName
                };
            }


        }
        [HttpGet]
        public ActionResult Detail(int id)
        {
            var db = new GosuslugiContext();
            var client = db.Clients.FirstOrDefault(x => x.Id == id);
            if (client == null)
                return RedirectPermanent("/Clients/Index");

            return View(client);

        }
    }
}