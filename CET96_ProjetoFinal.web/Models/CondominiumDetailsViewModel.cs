﻿using System.ComponentModel.DataAnnotations;

namespace CET96_ProjetoFinal.web.Models
{
    /// <summary>
    /// Represents the data needed for the Condominium Details view.
    /// This separates the database entity from the UI, providing a cleaner
    /// and more secure way to display information.
    /// </summary>
    public class CondominiumDetailsViewModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        [Display(Name = "Condominium Name")]
        public string Name { get; set; }

        public string Address { get; set; }

        [Display(Name = "Property Registry Number")]
        public string PropertyRegistryNumber { get; set; }

        [Display(Name = "Number of Units")]
        public int UnitsCount { get; set; }
        //public int NumberOfUnits { get; set; }

        [Display(Name = "Contract Value")]
        [DisplayFormat(DataFormatString = "{0:C}")] // Formats as currency
        public decimal ContractValue { get; set; }

        [Display(Name = "Fee Per Unit")]
        [DisplayFormat(DataFormatString = "{0:C}")] // Formats as currency
        public decimal FeePerUnit { get; set; }

        [Display(Name = "Date Created")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")] // Formats the date
        public DateTime CreatedAt { get; set; }
    }
}