﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoPartsWebSite.Models;

namespace AutoPartsWebSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SuppliersController : Controller
    {
        private SupplierModel db = new SupplierModel();

        // GET: Suppliers
        public ActionResult Index()
        {
            return View(db.Suppliers.ToList());
        }

        // GET: Suppliers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // GET: Suppliers/Create
        public ActionResult Create()
        {
            ViewBag.ImportTemplatesList = from importTemplate in db.ImportTemplates
                                          select new SelectListItem { Text = importTemplate.Name, Value = importTemplate.Id.ToString() };
            if (db.Suppliers.FirstOrDefault() != null)
            {
                ViewBag.TypesList = db.Suppliers.FirstOrDefault().getTypes();
            }
            else
            {
                List<SelectListItem> TypeItems = new List<SelectListItem>();
                TypeItems.Add(new SelectListItem
                {
                    Text = " Стандартный",
                    Value = "1"
                });
                TypeItems.Add(new SelectListItem
                {
                    Text = "Скрытый",
                    Value = "2",
                    Selected = true
                });
                ViewBag.TypesList = TypeItems;
            }

            return View();
        }

        // POST: Suppliers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Code,Rate,DeliveryTime,TypeId,ImportTemplateId")] Supplier supplier)
        {
            ViewBag.ImportTemplatesList = from importTemplate in db.ImportTemplates
                                          select new SelectListItem { Text = importTemplate.Name, Value = importTemplate.Id.ToString() };
            
            if (ModelState.IsValid)
            {
                db.Suppliers.Add(supplier);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TypesList = db.Suppliers.FirstOrDefault().getTypes();
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            ViewBag.ImportTemplatesList = from importTemplate in db.ImportTemplates
                                          select new SelectListItem { Text = importTemplate.Name, Value = importTemplate.Id.ToString() };
            ViewBag.TypesList = db.Suppliers.FirstOrDefault().getTypes();
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Code,Rate,DeliveryTime,TypeId, ImportTemplateId")] Supplier supplier)
        {
            ViewBag.ImportTemplatesList = from importTemplate in db.ImportTemplates
                                          select new SelectListItem { Text = importTemplate.Name, Value = importTemplate.Id.ToString() };
            //ViewBag.TypesList = db.Suppliers.FirstOrDefault().getTypes();
            if (ModelState.IsValid)
            {
                db.Entry(supplier).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Supplier supplier = db.Suppliers.Find(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }
            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Supplier supplier = db.Suppliers.Find(id);
            db.Suppliers.Remove(supplier);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
