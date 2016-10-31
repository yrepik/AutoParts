using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoPartsWebSite.Models;
using PagedList;
using IdentityAutoPart.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Postal;
using OfficeOpenXml;
using System.IO;

namespace AutoPartsWebSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdersAdminController : Controller
    {
        private OrderModel db = new OrderModel();
        
        // GET: OrdersAdmin
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.IdSortParm = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.StateSortParm = String.IsNullOrEmpty(sortOrder) ? "state_desc" : "State"; //"State" ? "state_desc" : "State";
            
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var orders = from s in db.Orders
                           select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(s => s.Id.ToString().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "id_desc":
                    orders = orders.OrderByDescending(s => s.Id);
                    break;
                case "Date":
                    orders = orders.OrderBy(s => s.Data);
                    break;
                case "date_desc":
                    orders = orders.OrderByDescending(s => s.Data);
                    break;
                case "State":
                    orders = orders.OrderBy(s => s.State);
                    break;
                case "state_desc":
                    orders = orders.OrderByDescending(s => s.State);
                    break;
                default:
                    orders = orders.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(orders.ToPagedList(pageNumber, pageSize));
        }

        // GET: OrdersAdmin/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

       

        // GET: OrdersAdmin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: OrdersAdmin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,Summary,Data,State")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // GET: OrdersAdmin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
                       
            ApplicationUserManager userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var users = userManager.Users.ToList();            
            ViewBag.Users = new SelectList(users, "Id", "FullName");

            return View(order);
        }

        // POST: OrdersAdmin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,Summary,Data,State")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                SendEmail(order); // send notifications
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(order);
        }

        // GET: OrdersAdmin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: OrdersAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //=============== OrderItems ====================

        public ActionResult IndexOrderItems(int? id)
        {
            if (TempData["shortMessage"] == null)
            {
                ViewBag.Message = "";
            }
            else
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //var orderItems = from s in db.OrderItems select s;
            ////var orderItems = db.OrderItems.Include(o => o.Order);
            //orderItems = orderItems.Where(s => s.Id.Equals(id));

            // var orderItems = db.OrderItems.Include(o => o.Order).Where(o => o.Order.Id.Equals(id));
            var orderItems = db.OrderItems.Include(o => o.Order)
                .Where(o => o.Order.Id == id);

            if (orderItems == null)
            {
                return HttpNotFound();
            }

            return View(orderItems.ToList());
        }

        public ActionResult EditOrderItems(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderItem orderItems = db.OrderItems.Find(id);
            if (orderItems == null)
            {
                return HttpNotFound();
            }
            return View(orderItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public ActionResult EditOrderItems([Bind(Include = "Id,OrderId,PartId,UserId,Brand,Number,Name,Details,Size,Weight,Quantity,Price,Supplier,DeliveryTime,Amount,Data,State, Reference1, Reference2")] OrderItem orderItem)
        //public ActionResult EditOrderItems([Bind(Include = "Id,OrderId,PartId,UserId,Brand,Number,Name,Details,Amount,Data,State")] OrderItem orderItem)
        // public ActionResult EditOrderItems([Bind(Include = "Brand,Number,Name,Details,Size,Weight,Quantity,Price,Supplier,DeliveryTime,Amount,Data,State")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderItem).State = EntityState.Modified;                
                Order order = db.Orders.Find(orderItem.OrderId);                 
                order.updateSummary(); // update order summary
                SendEmail(db.Orders.Find(orderItem.OrderId)); // send notifications
                db.SaveChanges();
                return RedirectToAction("IndexOrderItems", new { id = orderItem.OrderId });
            }
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "UserId", orderItem.OrderId);
            return View(orderItem);
        }

        // GET: OrdersAdmin/Delete/5
        public ActionResult DeleteOrderItems(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderItem orderItem = db.OrderItems.Find(id);
            if (orderItem == null)
            {
                return HttpNotFound();
            }
            return View(orderItem);
        }

        // POST: OrdersAdmin/Delete/5
        [HttpPost, ActionName("DeleteOrderItems")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteOrderItemsConfirmed(int id)
        {
            OrderItem orderItem = db.OrderItems.Find(id);
            Order order = db.Orders.Find(orderItem.OrderId);
            db.OrderItems.Remove(orderItem);
            order.updateSummary();
            SendEmail(db.Orders.Find(orderItem.OrderId)); // send notifications
            db.SaveChanges();                      
            return RedirectToAction("Index");
        }

        public int GetOrderItemsCont(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return -1;
            }

            var orderItems = db.OrderItems.Include(o => o.Order)
               .Where(o => o.Order.Id == id);            
            if (orderItems == null)
            {
                return 0;
            }

            return orderItems.Count();
        }

        public ActionResult ExcelExport(int? id)
        {
            string currentUserId = User.Identity.GetUserId();
            var userOrderItems = from oi in db.OrderItems
                                 where oi.OrderId == id
                                 select new { oi.Id, oi.OrderId, oi.Number, oi.Brand, oi.Details, oi.DeliveryTime, oi.Amount, oi.Price, oi.State, oi.Supplier, oi.Reference1, oi.Reference2 };

            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("ALFAPARTS-Order-" + id.ToString());
                ws.Cells["A1"].LoadFromCollection(userOrderItems, true);
                // ToDo: еще нужно будет добавить русские хидеры
                Byte[] fileBytes = pck.GetAsByteArray();
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=ALFAPARTS-Order " + id.ToString() + ".xlsx");
                // Заменяю имя выходного Эксель файла
                Response.Charset = "";
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                Response.BinaryWrite(fileBytes);
                Response.End();
            }
            TempData["shortMessage"] = "<br> Экспорт завершен";
            return RedirectToAction("Index");
        }
        //========end======= OrderItems =======end=============

        public void SendEmail(Order neworder)
        {
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(neworder.UserId);

            // send new order e-mail to admin
            dynamic adminNewOrder = new Email("adminChangeOrder");
            adminNewOrder.To = "admins@alfa-parts.com";
            adminNewOrder.Order = neworder.Id;
            adminNewOrder.Send();

            // send new order e-mail to user
            dynamic userNewOrder = new Email("userChangeOrder");
            userNewOrder.To = user.Email;
            adminNewOrder.Order = neworder;
            userNewOrder.Send();
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
