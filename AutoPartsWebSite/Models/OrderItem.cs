﻿namespace AutoPartsWebSite.Models
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
    using System.Web.Mvc;
    [Table("OrderItem")]
    public partial class OrderItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        //public OrderItem()
        //{
        //    InvoiceOrderItems = new HashSet<InvoiceOrderItem>();
        //}

        [Key]
        public int Id { get; set; }

        [ForeignKey("Order")]
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int PartId { get; set; }

        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        [Display(Name = "Марка")]
        public string Brand { get; set; }

        [Display(Name = "Номер")]
        public string Number { get; set; }

        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Details { get; set; }

        [Display(Name = "Объем")]
        public string Size { get; set; }

        [Display(Name = "Вес")]
        public string Weight { get; set; }

        [Display(Name = "Наличие")]
        public string Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Display(Name = "Цена")]
        public string Price { get; set; }

        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Display(Name = "Базовая цена")]
        public string BasePrice { get; set; }

        [Display(Name = "Ref1")]
        [StringLength(10)]
        public string Reference1 { get; set; }

        [Display(Name = "Ref2")]
        [StringLength(10)]
        public string Reference2 { get; set; }

        [Display(Name = "Поставщик")]
        public string Supplier { get; set; }

        [Display(Name = "Срок поставки")]
        public string DeliveryTime { get; set; }
        [Display(Name = "Количество")]
        public int? Amount { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]
        public DateTime? Data { get; set; }

        [Display(Name = "Статус")]
        public int State { get; set; }

        //[Display(Name = "Статус")]
        //public string StateText
        //{
        //    get
        //    {
        //        var itemS = getOrderItemStates();
        //        string state = this.State.ToString();
        //        return itemS.Find(x => x.Value.Contains(state)).Text;
        //    }            
        //}

        [Display(Name = "Стоимость")]
        public decimal? Total { get { return Amount * (Convert.ToDecimal(Price)); } }

        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [Display(Name = "Цена")]
        public decimal? PriceRnd { get { return Convert.ToDecimal(Price); } }

        public virtual Order Order { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<InvoiceOrderItem> InvoiceOrderItems { get; set; }        

        public List<SelectListItem> getOrderItemStates()
        {
            List<SelectListItem> StateItems = new List<SelectListItem>();
            StateItems.Add(new SelectListItem
            {
                Text = "В работе",
                Value = "1"
            });
            StateItems.Add(new SelectListItem
            {
                Text = "Закуплено",
                Value = "2",
                Selected = true
            });
            StateItems.Add(new SelectListItem
            {
                Text = "Снято",
                Value = "3"
            });
            StateItems.Add(new SelectListItem
            {
                Text = "Отправлено",
                Value = "4"
            });
            StateItems.Add(new SelectListItem
            {
                Text = "Готово к выдаче",
                Value = "5"
            });
            StateItems.Add(new SelectListItem
            {
                Text = "Выдано",
                Value = "6"
            });
            return StateItems;
        }

        public string getStateName()
        {
            var itemS = getOrderItemStates();
            string state = this.State.ToString();
            return itemS.Find(x => x.Value.Contains(state)).Text;         
        }

        [Display(Name = "Заказчик")]
        [StringLength(128)]
        public string UserName
        {
            get
            {
                ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var user = userManager.FindById(UserId);
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
        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
    }
}

