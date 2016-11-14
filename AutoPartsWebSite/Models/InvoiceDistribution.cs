namespace AutoPartsWebSite.Models
{
    using IdentityAutoPart.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web;

    [Table("InvoiceDistribution")]
    public partial class InvoiceDistribution
    {
        public int Id { get; set; }

        [Display(Name = "Номер инвойса")]
        public int invoiceId
        {
            get
            {
                InvoiceModel db = new InvoiceModel();
                InvoiceItem invoiceItem = db.InvoiceItems.Find(InvoiceItemId);
                if (invoiceItem == null)
                {
                    return 0;
                }
                return invoiceItem.InvoiceId;

            }
        }

        //[Display(Name = "Статус инвойса")]
        //public int invoiceState
        //{
        //    get
        //    {
        //        InvoiceModel db = new InvoiceModel();
        //        InvoiceItem invoiceItem = db.InvoiceItems.Find(InvoiceItemId);
        //        if (invoiceItem == null)
        //        {
        //            return 0;
        //        }
        //        Invoice invoice = db.Invoices.Find(invoiceItem.InvoiceId);
        //        if (invoice == null)
        //        {
        //            return 0;
        //        }
        //        return invoice.State;
        //    }
        //}

        [Display(Name = "Номер позиции инвойса")]
        public int InvoiceItemId { get; set; }

        [Display(Name = "Номер позиции заказа")]
        public int OrderItemId { get; set; }

        [Display(Name = "К-во")]
        public int Quantity { get; set; }

        [Display(Name = "Номер заказа")]
        public int OrderId
        {
            get
            {
                OrderModel db = new OrderModel();
                OrderItem orderItem = db.OrderItems.Find(OrderItemId);                
                if (orderItem == null)
                {
                    return 0;
                }
                return orderItem.OrderId;

            }
        }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата заказа")]
        public DateTime? OrderDate
        {
            get
            {
                OrderModel db = new OrderModel();
                Order order = db.Orders.Find(OrderId);
                if (order == null)
                {
                    return null;
                }
                return order.Data;

            }
        }

        [StringLength(128)]
        public string OrderUserId
        {
            get
            {
                OrderModel db = new OrderModel();
                Order order = db.Orders.Find(OrderId);
                if (order == null)
                {
                    return null;
                }
                return order.UserId;

            }
        }

        [Display(Name = "Заказчик")]
        [StringLength(128)]
        public string UserName
        {
            get
            {
                ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = userManager.FindById(OrderUserId);
                if (user != null)
                {
                    return user.FullName;
                }
                else
                {
                    return "";
                }
            }
        }
    }
}
