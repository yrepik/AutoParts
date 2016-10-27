using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoPartsWebSite.Models;
using Microsoft.AspNet.Identity;
using System.IO;
using OfficeOpenXml;
using PagedList;

namespace AutoPartsWebSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InvoicesController : Controller
    {
        private InvoiceModel db = new InvoiceModel();

        // GET: Invoices
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NumberSortParm = String.IsNullOrEmpty(sortOrder) ? "number_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            var invoices = from s in db.Invoices
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                invoices = invoices.Where(s => s.Number.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "number_desc":
                    invoices = invoices.OrderByDescending(s => s.Number);
                    break;
                case "Date":
                    invoices = invoices.OrderBy(s => s.Date);
                    break;
                case "date_desc":
                    invoices = invoices.OrderByDescending(s => s.Date);
                    break;
                default:
                    invoices = invoices.OrderBy(s => s.Number);
                    break;
            }
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(invoices.ToPagedList(pageNumber, pageSize));
            //return View(db.Invoices.ToList());
        }

        // GET: Invoices/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }
        public ActionResult DetailsInvoiceItem(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvoiceItem invoiceItem = db.InvoiceItems.Find(id);
            if (invoiceItem != null)
            {
                return PartialView("DetailsInvoiceItem", invoiceItem);
            }
            return View(invoiceItem);
        }

        // GET: Invoices/Create
        public ActionResult Create()
        {
            ViewBag.UserId = User.Identity.GetUserId();
            Invoice invoice = new Invoice();
            ViewBag.SuppliersList = from supplier in db.Suppliers
                                    select new SelectListItem { Text = supplier.Name, Value = supplier.Id.ToString() };
            return View();
        }

        // POST: Invoices/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,Date,Number,State,SupplierId,FileName")] Invoice invoice, HttpPostedFileBase upload)
        {
            ViewBag.SuppliersList = from supplier in db.Suppliers
                                    select new SelectListItem { Text = supplier.Name, Value = supplier.Id.ToString() };
            if (ModelState.IsValid)
            {
                try
                {
                    invoice.UserId = User.Identity.GetUserId();
                    //invoice.Date = System.DateTime.Now;
                    invoice.FileName = System.IO.Path.GetFileName(upload.FileName);
                    invoice.State = 1;
                    // store data into DB
                    db.Invoices.Add(invoice);
                    db.SaveChanges();
                    // parse data from XSLT file and store it into DB
                    if (ExcelImport(invoice.Id, upload))
                    {
                        ViewBag.Message = "Загружено.";
                        return RedirectToAction("Index");
                    }
                    //TempData["shortMessage"] = "Загружено.";
                    //return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Ошибка загрузки:" + ex.Message.ToString();
                    return View(invoice);
                }
            }
            return View(invoice);
        }

        // GET: Invoices/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            ViewBag.SuppliersList = from supplier in db.Suppliers
                                    select new SelectListItem { Text = supplier.Name, Value = supplier.Id.ToString() };
            return View(invoice);
        }

        // POST: Invoices/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,Date,Number,State,SupplierId,FileName,LinesNumber")] Invoice invoice, HttpPostedFileBase upload)
        {
            ViewBag.SuppliersList = from supplier in db.Suppliers
                                    select new SelectListItem { Text = supplier.Name, Value = supplier.Id.ToString() };
            if (ModelState.IsValid)
            {
                invoice.UserId = User.Identity.GetUserId();
                //invoice.Date = System.DateTime.Now;
                if (upload != null && upload.ContentLength > 0)
                {
                    invoice.FileName = System.IO.Path.GetFileName(upload.FileName);
                    if (ExcelImport(invoice.Id, upload))
                    {
                        TempData["shortMessage"] = "Загружено.";                     
                    }
                }
                db.Entry(invoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(invoice);
        }

        // GET: Invoices/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                return HttpNotFound();
            }
            return View(invoice);
        }

        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invoice invoice = db.Invoices.Find(id);
            var invoiceItems = from c in db.InvoiceItems where c.InvoiceId.Equals(invoice.Id) select c;
            db.InvoiceItems.RemoveRange(invoiceItems);
            db.Invoices.Remove(invoice);            
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        private bool ExcelImport(int invoiceId, HttpPostedFileBase upload)
        {
            int firstDataRow = 2;
            int linesNumber = 0;
            try
            {
                Invoice invoice = db.Invoices.Find(invoiceId);
                if (upload != null && upload.ContentLength > 0)
                {
                    // clear the table InvoiceItem by InvoiceId
                    var all = from c in db.InvoiceItems where c.InvoiceId.Equals(invoiceId) select c;
                    db.InvoiceItems.RemoveRange(all);
                    // load from stream
                    using (ExcelPackage package = new ExcelPackage(upload.InputStream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        for (int i = firstDataRow; i <= worksheet.Dimension.End.Row; i++)
                        {
                            InvoiceItem invoiceItem = new InvoiceItem
                            {
                                InvoiceId = invoiceId,
                                Date = System.DateTime.Now,
                                Number = worksheet.Cells["A" + i.ToString()].Value.ToString(),
                                Brand = worksheet.Cells["B" + i.ToString()].Value.ToString(),
                                Quantity = Convert.ToInt32( worksheet.Cells["C" + i.ToString()].Value.ToString()),
                                State = 1
                            };
                            db.InvoiceItems.Add(invoiceItem);
                            linesNumber++;
                        }
                    }
                    invoice.LinesNumber = linesNumber;
                    db.SaveChanges();
                    ViewBag.Message = "Загружено.";
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка загрузки:" + ex.Message.ToString();

                return false;
            }
        }

        // DO: Distribution
        public ActionResult Distribution(int? id)
        {
            if (id == null)
            {
                RedirectToAction("Index");
            }

            Invoice invoice = db.Invoices.Find(id);
            if (invoice == null)
            {
                RedirectToAction("Index");
            }
            var invoiceItems = db.InvoiceItems.Include(o => o.Invoice)
                .Where(o => o.Invoice.Id == id);            
            foreach (InvoiceItem iItem in invoiceItems)
            {
                if (DistributeInvoiceItem(iItem.Id))
                {
                    invoice.State = 3;                     
                }
            }
            db.SaveChanges();
            return RedirectToAction("IndexInvoiceItems", new { id = id });            
        }

        public ActionResult IndexInvoiceItems(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invoice invoice = db.Invoices.Find(id);
            
            var invoiceItems = db.InvoiceItems.Include(o => o.Invoice)
                .Where(o => o.Invoice.Id == id);

            if (invoiceItems == null)
            {
                return HttpNotFound();
            }
            ViewBag.CurrentInvoice = invoice;
            return View(invoiceItems.ToList());
        }

        private Boolean DistributeInvoiceItem(int id)
        {
            try
            {
                // cleare Distribution table by InvoiceItemId
                var invDistr = from d in db.InvoiceDistributions where d.InvoiceItemId.Equals(id) select d;
                db.InvoiceDistributions.RemoveRange(invDistr);

                InvoiceItem invoiceItem = db.InvoiceItems.Find(id);
                if (invoiceItem == null)
                {
                    return false;
                }
                int invoiceItemQuantity = Convert.ToInt32(invoiceItem.Quantity);
                int OrderItemAmount = -1;
                var orderItems = (from oi in db.OrderItems
                                  where (oi.Number == invoiceItem.Number
                                          && oi.Brand == invoiceItem.Brand
                                          && oi.State == 1
                                        )
                                  select oi)
                                .ToList()
                                .OrderByDescending(ou => ou.Amount)
                                .ThenByDescending(ou => ou.Data);
                foreach (OrderItem oItem in orderItems)
                {
                    OrderItemAmount = Convert.ToInt32(oItem.Amount);
                    if (invoiceItemQuantity >= OrderItemAmount)
                    {
                        // add OrderItemId to Distribution table
                        if (ModelState.IsValid)
                        {
                            InvoiceDistribution invoiceDistribution = new InvoiceDistribution
                            {
                                InvoiceItemId = invoiceItem.Id,
                                OrderItemId = oItem.Id,
                                Quantity = Convert.ToInt32(oItem.Amount)
                            };
                            db.InvoiceDistributions.Add(invoiceDistribution);
                            invoiceItemQuantity = invoiceItemQuantity - OrderItemAmount;                            
                        }
                    }
                }
                if (OrderItemAmount == -1)       // not distributed
                {
                    invoiceItem.State = 1;
                }
                if (invoiceItemQuantity == 0)    // distributed
                {
                    invoiceItem.State = 2;
                }
                if (invoiceItemQuantity != 0 && OrderItemAmount > 0)    // distributed with rest
                {
                    invoiceItem.State = 3;      
                }
                return true;
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Ошибка:" + ex.Message.ToString();
                return false;
            }
        }

        public ActionResult IndexInvoiceDistribution(int? id, int invoiceItemRest)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var invoiceDistributions = from c in db.InvoiceDistributions where c.InvoiceItemId == id select c;

            ViewBag.InvoiceDistributionsCount = invoiceDistributions.Count();
            ViewBag.invoiceItemRest = invoiceItemRest;
            ViewBag.InvoiceItemId = id;
            return PartialView(invoiceDistributions.ToList());
        }

        public ActionResult DetailsInvoiceDistribution(int id)
        {
            InvoiceDistribution invoiceDistribution = db.InvoiceDistributions.Find(id);
            if (invoiceDistribution != null)
            {
                return PartialView("Details", invoiceDistribution);
            }
            return View("IndexInvoiceItems");
        }

        // GET: InvoiceDistribution/Create
        public ActionResult CreateInvoiceDistribution(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvoiceItem invoiceItem = db.InvoiceItems.Find(id);
            if (invoiceItem == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.InvoiceId = invoiceItem.InvoiceId;
            ViewBag.invoiceItemRest = invoiceItem.Rest;

            ViewBag.OrderItemsList = from orderItems in db.OrderItems
                                     where (orderItems.Number == invoiceItem.Number
                                            && orderItems.Brand == invoiceItem.Brand
                                            && orderItems.State == 1
                                            )
                                     select new SelectListItem { Text = "Заказ № " + orderItems.OrderId.ToString() 
                                                                    + " позиция # " + orderItems.Id.ToString()
                                                                    + " к-во: " + orderItems.Amount.ToString()
                                                                    //+ " от " + orderItems.Data.Value.ToString("dd.MM.yyyy")
                                                                    //+ " - " + orderItems.UserName.ToString()
                                                                    , Value = orderItems.Id.ToString() };
            return View();
        }

        // POST: InvoiceDistribution/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateInvoiceDistribution([Bind(Include = "Id,InvoiceItemId,OrderItemId,Quantity")] InvoiceDistribution invoiceDistribution)
        {
            if (ModelState.IsValid)
            {
                invoiceDistribution.InvoiceItemId = invoiceDistribution.Id;
                db.InvoiceDistributions.Add(invoiceDistribution);
                db.SaveChanges();

                return RedirectToAction("IndexInvoiceItems", new { id = invoiceDistribution.invoiceId });
            }

            return View(invoiceDistribution);
        }

        // GET: InvoiceDistribution/Delete/5
        public ActionResult DeleteInvoiceDistribution(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvoiceDistribution invoiceDistribution = db.InvoiceDistributions.Find(id);
            if (invoiceDistribution == null)
            {
                return HttpNotFound();
            }
            return View(invoiceDistribution);
        }

        // POST: InvoiceDistribution/Delete/5
        [HttpPost, ActionName("DeleteInvoiceDistribution")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedInvoiceDistribution(int id)
        {
            InvoiceDistribution invoiceDistribution = db.InvoiceDistributions.Find(id);
            int invoiceId = invoiceDistribution.invoiceId;
            db.InvoiceDistributions.Remove(invoiceDistribution);
            db.SaveChanges();
            return RedirectToAction("IndexInvoiceItems", new { id = invoiceId });
        }

        // GET: InvoiceDistribution/Edit/5
        public ActionResult EditInvoiceDistribution(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvoiceDistribution invoiceDistribution = db.InvoiceDistributions.Find(id);
            if (invoiceDistribution == null)
            {
                return HttpNotFound();
            }
            return View(invoiceDistribution);
        }

        // POST: InvoiceDistribution/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInvoiceDistribution([Bind(Include = "Id,InvoiceItemId,OrderItemId,Quantity")] InvoiceDistribution invoiceDistribution)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invoiceDistribution).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("IndexInvoiceItems", new { id = invoiceDistribution.invoiceId });
            }
            return View(invoiceDistribution);
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
