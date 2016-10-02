namespace AutoPartsWebSite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Web.Mvc;

    [Table("Supplier")]
    public partial class Supplier
    {
        public int Id { get; set; }

        [Display(Name = "Название")]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "Код")]
        [StringLength(50)]
        public string Code { get; set; }

        [Display(Name = "Базовая наценка %")]
        [Column(TypeName = "numeric")]
        public decimal? Rate { get; set; }

        [Display(Name = "Срок поставки")]
        [StringLength(10)]
        public string DeliveryTime { get; set; }

        public int ImportTemplateId { get; set; }
        [Display(Name = "Шаблон импорта")]
        public string ImportTemplate
        {
            get
            {
                ImportTemplateModel db = new ImportTemplateModel();
                ImportTemplate importTemplate = db.ImportTemplates.Find(ImportTemplateId);
                if (importTemplate == null)
                {
                    return "";
                }
                return importTemplate.Name.ToString();
            }
        }

        [Display(Name = "Тип")]
        public int TypeId { get; set; }
        [Display(Name = "Тип")]
        public string Type {
            get
            {                
                return getTypes().Find(x => x.Value.Equals(TypeId.ToString())).Text;
            }
        }
        public List<SelectListItem> getTypes()
        {
            List<SelectListItem> StateItems = new List<SelectListItem>();
            StateItems.Add(new SelectListItem
            {
                Text = " Стандартный",
                Value = "1"
            });
            StateItems.Add(new SelectListItem
            {
                Text = "Скрытый",
                Value = "2",
                Selected = true
            });           
            return StateItems;
        }
    }
}
