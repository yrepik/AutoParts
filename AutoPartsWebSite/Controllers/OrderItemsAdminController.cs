using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AutoPartsWebSite.Models;
using Postal;
using Microsoft.AspNet.Identity.Owin;
using IdentityAutoPart.Models;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using System.IO;

namespace AutoPartsWebSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderItemsAdminController : Controller
    {
        private OrderModel db = new OrderModel();

        // GET: OrderItemsAdmin
        public ActionResult Index()
        {
            var orderItems = db.OrderItems.Include(o => o.Order);
            return View(orderItems.ToList());
        }

        // GET: OrderItemsAdmin/Details/5
        public ActionResult Details(int? id)
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

        // GET: OrderItemsAdmin/Create
        public ActionResult Create()
        {
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "UserId");
            return View();
        }

        // POST: OrderItemsAdmin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,OrderId,PartId,UserId,Brand,Number,Name,Details,Size,Weight,Quantity,Price,Supplier,DeliveryTime,Amount,Data,State")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                db.OrderItems.Add(orderItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = new SelectList(db.Orders, "Id", "UserId", orderItem.OrderId);
            return View(orderItem);
        }

        // GET: OrderItemsAdmin/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "UserId", orderItem.OrderId);
            return View(orderItem);
        }

        // POST: OrderItemsAdmin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,OrderId,PartId,UserId,Brand,Number,Name,Details,Size,Weight,Quantity,Price,Supplier,DeliveryTime,Amount,Data,State")] OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                // check state and send an e-mail
                //var original = db.OrderItems.Find(orderItem.Id);
                //if (original.State != orderItem.State)
                //{
                //    SendEmail(orderItem);
                //}

                db.Entry(orderItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "UserId", orderItem.OrderId);
            return View(orderItem);
        }

        // GET: OrderItemsAdmin/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: OrderItemsAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderItem orderItem = db.OrderItems.Find(id);
            db.OrderItems.Remove(orderItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public int getOrderItemState(OrderItem orderItem)
        {
            if (orderItem == null)
            {
                return 0;
            }
            else
            {
                return orderItem.State;
            }
        }
        private List <int> updatedItems { get; set; }

        public int setOrderItemState(int id, int newstate)
        {
            int returnValue = 0;

            OrderItem orderItem = db.OrderItems.Find(id);
            if (orderItem == null)
            {
                return returnValue;
            }

            if (orderItem.State == newstate)
            {
                return returnValue;
            }

            returnValue = (orderItem.State < newstate) ? 1 : -1;           

            if (ModelState.IsValid)
            {
                orderItem.State = newstate;
                db.Entry(orderItem).State = EntityState.Modified;
                db.SaveChanges();
            }
            this.updatedItems.Add(id); // add update items id into the list
            // send send an e-mail about order item state changes
            // SendEmail(orderItem);
            return returnValue;
        }

        public void SendUpdatedItemsEmail()
        {
            var orderItemsGroups = from oi in db.OrderItems
                            where updatedItems.Contains(oi.Id)
                            group oi by oi.UserId into g
                            select new
                            {
                                Name = g.Key,
                                Count = g.Count(),
                                Items = from i in g select i
                            };

            foreach (var group in orderItemsGroups)
            {
                //Console.WriteLine("{0} : {1}", group.Name, group.Count);
                //foreach (OrderItem item in group.Items)
                //    Console.WriteLine(item.Name);
                //Console.WriteLine();

                var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(group.Items.FirstOrDefault().UserId);
                // send e-mail to user
                dynamic userChangeOrderItemGroup = new Email("userChangeOrderItemGroup");
                userChangeOrderItemGroup.To = user.Email;
                //userNewOrder.Order = orderItem.OrderId;
                userChangeOrderItemGroup.OrderItems = group.Items;
                userChangeOrderItemGroup.OrderId = group.Items.FirstOrDefault().Order.Id;
                userChangeOrderItemGroup.OrderDate = group.Items.FirstOrDefault().Order.Data;
                userChangeOrderItemGroup.OrderSummary = group.Items.FirstOrDefault().Order.Summary;

                //userNewOrder.OrderItemState = orderItem.State;
                userChangeOrderItemGroup.Send();
            }


            //var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(orderItem.UserId);
            // send e-mail to admin

            //dynamic adminNewOrder = new Email("adminChangeOrderItem");
            //adminNewOrder.To = "admins@alfa-parts.com";
            //adminNewOrder.Order = orderItem.OrderId;
            //adminNewOrder.OrderItem = orderItem.Id;
            //adminNewOrder.OrderItemState = orderItem.State;
            //adminNewOrder.Send();

            // send e-mail to user
            //dynamic userNewOrder = new Email("userChangeOrderItem");
            //userNewOrder.To = user.Email;
            //userNewOrder.Order = orderItem.OrderId;
            //userNewOrder.OrderItem = orderItem.Id;
            //userNewOrder.OrderItemState = orderItem.State;
            //userNewOrder.Send();
        }

        public void SendEmail(OrderItem orderItem)
        {
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(orderItem.UserId);
            // send e-mail to admin

            //dynamic adminNewOrder = new Email("adminChangeOrderItem");
            //adminNewOrder.To = "admins@alfa-parts.com";
            //adminNewOrder.Order = orderItem.OrderId;
            //adminNewOrder.OrderItem = orderItem.Id;
            //adminNewOrder.OrderItemState = orderItem.State;
            //adminNewOrder.Send();

            // send e-mail to user
            dynamic userChangeOrderItem = new Email("userChangeOrderItem");
            userChangeOrderItem.To = user.Email;
            userChangeOrderItem.OrderId = orderItem.OrderId;
            userChangeOrderItem.OrderDate = orderItem.Order.Data;
            userChangeOrderItem.OrderSummary = orderItem.Order.Summary;
            userChangeOrderItem.OrderItem = orderItem.Id;
            userChangeOrderItem.OrderItemState = orderItem.State;
            userChangeOrderItem.Send();
        }

        public ActionResult IndexState()
        {
            if (TempData["shortMessage"] == null)
            {
                ViewBag.Message = "";
            }
            else
            {
                ViewBag.Message = TempData["shortMessage"].ToString();
            }

            var orderItems = from s in db.OrderItems
                             where s.State < 6
                             select s;
            return View(orderItems.ToList());
        }
        public ActionResult ExcelExportState()
        {
            var orderItems = from s in db.OrderItems
                                where s.State < 6
                                select new { s.Id , s.State, s.OrderId, s.Number, s.Name, s.Details };
            using (ExcelPackage pck = new ExcelPackage())
            {
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("ALFAPARTS-OrderItemsState");
                ws.Cells["A1"].LoadFromCollection(orderItems, true);                
                // ToDo: еще нужно будет добавить русские хидеры
                Byte[] fileBytes = pck.GetAsByteArray();
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=ALFAPARTS-OrderItemsState.xlsx");
                // Заменяю имя выходного Эксель файла
                Response.Charset = "";
                Response.ContentType = "application/vnd.ms-excel";
                StringWriter sw = new StringWriter();
                Response.BinaryWrite(fileBytes);
                Response.End();
            }
            TempData["shortMessage"] = "<br> Экспорт завершен";
            return RedirectToAction("IndexState");
        }

        public ActionResult ExcelImportState()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExcelImportState(HttpPostedFileBase upload)
        {
            int firstDataRow = 2;
            int orderItemId;
            int orderItemState;
            string infoMessage = "";
            try
            {
                if (upload != null && upload.ContentLength > 0)
                {                    
                    using (ExcelPackage package = new ExcelPackage(upload.InputStream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        this.updatedItems = new List<int> { };
                        for (int i = firstDataRow; i <= worksheet.Dimension.End.Row; i++)
                        {
                            orderItemId = Convert.ToInt32(worksheet.Cells["A" + i.ToString()].Value.ToString());
                            orderItemState = Convert.ToInt32(worksheet.Cells["B" + i.ToString()].Value.ToString());
                            if (orderItemState > 6) orderItemState = 6;
                            if (setOrderItemState(orderItemId, orderItemState)  < 0)
                            {
                                infoMessage = "Позиция " + orderItemId + "  - произошло уменьшение статуса <br>";
                            }                            
                        }
                    }
                    TempData["shortMessage"] = infoMessage + "<br> Импорт завершен.";
                    SendUpdatedItemsEmail();
                }
                return RedirectToAction("IndexState");
            }
            catch (Exception ex)
            {
                TempData["shortMessage"] = "Ошибка импорта:" + ex.Message.ToString();

                return RedirectToAction("IndexState");
            }
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
