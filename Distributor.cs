using Nop.Core;
using Nop.Core.Domain.Localization;
using SDK.IT.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDK.IT.Core.Domain.Business
{
    public enum DistributorType
    {
        Entrepreneur,
        Customer
    }
	
	//comments added just to make some changes.

    public abstract class Distributor : BaseEntity
    {
        public abstract DistributorType PersonType { get; }

        public int? SponsorId { get; set; }

        public int? UplineId { get; set; }

        public virtual Contact ContactRef { get; set; }


        public virtual Person PersonalDetails { get; set; }

        public virtual DistributorTaxDetails D_TaxDetails { get; set; }

        public int SourceTypeId { get; set; }

        public virtual DistributorInformationSourceType SourceType { get; set; }

        public int PreferredLanguageTypeId { get; set; }

        public virtual Language PreferredLanguageType { get; set; }

        public int BusinessTypeId { get; set; }

        public virtual DistributorBusinessType BusinessType { get; set; }
                
        public virtual DistributorWebAdministration WebAdministration { get; set; }

        public virtual DistributorActivityLog ActivityLog { get; set; }

        public virtual DistributorBankDetails BankDetails { get; set; }

        public virtual Distributor Sponsor { get; set; }

        public virtual Distributor Upline { get; set; }

        public virtual ICollection<Distributor> DownlineList { get; set; }

        public virtual ICollection<Distributor> SponsoredList { get; set; }

        public virtual ICollection<DistributorEntityLog> LogDetails { get; set; }         
    }
}
