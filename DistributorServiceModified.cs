using System;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using SDK.IT.Core.Domain.Business;
using System.Configuration;
using SDK.IT.Services.Common;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Dynamic;
using System.Web.Mvc;

namespace SDK.IT.Services.Business
{
    public class DistributorService:IDistributorService
    {
        static readonly string CACHE_INFORMATIONSOURCE_TYPE_LIST = "IT_Cache_DistributorInformationSourceType_List";
        static readonly string CACHE_BUSINESS_TYPE_LIST = "IT_Cache_DistributorBusinessType_List";
        static readonly string CACHE_PAY_TYPE_LIST = "IT_Cache_DistributorPayType_List";

        private readonly ICacheManager _cacheManager;

        private readonly IDistributorEntityLogService _logService;
        
        private readonly IRepository<Distributor> _distributorRepository;
        private readonly IRepository<DistributorPayType> _payTypeRepository;
        private readonly IRepository<DistributorBusinessType> _businessTypeRepository;
        private readonly IRepository<DistributorInformationSourceType> _informationSourceTypeRepository;

        const string SEARCH_BY_ID = "ID";
        const string SEARCH_BY_FIRSTNAME = "First Name";
        const string SEARCH_BY_LASTNAME = "Last Name";
        const string SEARCH_BY_TAXID = "Tax Id";
        const string SEARCH_BY_PHONENUM = "Phone#(USA/CAN)";
        const string SEARCH_BY_CITY = "City";
        const string SEARCH_BY_ZIP = "Zip";
        const string SEARCH_BY_COUNTRY = "Country";
        const string SEARCH_BY_STATE = "State";
        const string SEARCH_BY_EMAIL = "Email";

        const string VALUE_OF_ID            = "CustomerCode";
        const string VALUE_OF_FIRSTNAME     = "FirstName";
        const string VALUE_OF_LASTNAME      = "LastName";
        const string VALUE_OF_TAXID         = "TaxId";
        const string VALUE_OF_PHONENUM      = "MobilePhone";
        const string VALUE_OF_CITY          = "City";
        const string VALUE_OF_ZIP           = "ZipCode";
        const string VALUE_OF_COUNTRY       = "ThreeLetterIsoCode";
        const string VALUE_OF_STATE         = "Abbreviation";
        const string VALUE_OF_EMAIL         = "Email";
       

        static readonly List<SelectListItem> s_searchOptions = new List<SelectListItem>(){
                                            
                                new SelectListItem { Text = SEARCH_BY_ID       , Value = VALUE_OF_ID       },
                                new SelectListItem { Text = SEARCH_BY_FIRSTNAME, Value = VALUE_OF_FIRSTNAME} ,
                                new SelectListItem { Text = SEARCH_BY_LASTNAME , Value = VALUE_OF_LASTNAME },
                                new SelectListItem { Text = SEARCH_BY_TAXID    , Value = VALUE_OF_TAXID    },
                                new SelectListItem { Text = SEARCH_BY_PHONENUM , Value = VALUE_OF_PHONENUM },
                                new SelectListItem { Text = SEARCH_BY_CITY     , Value = VALUE_OF_CITY     } ,
                                new SelectListItem { Text = SEARCH_BY_ZIP      , Value = VALUE_OF_ZIP      },
                                new SelectListItem { Text = SEARCH_BY_COUNTRY  , Value = VALUE_OF_COUNTRY  } ,
                                new SelectListItem { Text = SEARCH_BY_STATE    , Value = VALUE_OF_STATE    },
                                new SelectListItem { Text = SEARCH_BY_EMAIL    , Value = VALUE_OF_EMAIL     }
            };

        public DistributorService(ICacheManager cacheManager,
            IDistributorEntityLogService logService,
            IRepository<Distributor> distributorRepository,
            IRepository<DistributorInformationSourceType> informationSourceTypeRepository,
            IRepository<DistributorBusinessType> businessTypeRepository,
            IRepository<DistributorPayType> payTypeRepository )
        {
            _cacheManager = cacheManager;
            _logService = logService;
            _distributorRepository = distributorRepository;

            _informationSourceTypeRepository = informationSourceTypeRepository;                
            _businessTypeRepository = businessTypeRepository;
            _payTypeRepository = payTypeRepository;        
        }

        #region IDistributorService Members

        public List<SelectListItem> GetDistributorSearchOptions()
        {
            return s_searchOptions;
        }

        public IEnumerable<DistributorSearchResult> GetSearchResults(string searchType, string searchText)
        {
            var queryRequiredData = from d in _distributorRepository.Table
                                    select new DistributorSearchResult()
                                    {
                                        Id = d.Id,
                                        FirstName = d.PersonalDetails.FirstName,
                                        LastName = d.PersonalDetails.LastName,
                                        Email = d.ContactRef.Email,
                                        CustomerCode = d.DistributorCode,
                                        IsActive = d.IsActive,
                                        City = d.ContactRef.AddressBookRef.BillingAddress.City,
                                        ZipCode = d.ContactRef.AddressBookRef.BillingAddress.ZipCode,
                                        MobilePhone = d.ContactRef.PhoneBookRef.MobilePhone,
                                        TaxId = d.BankDetails.TaxId,
                                        ThreeLetterIsoCode = d.ContactRef.AddressBookRef.BillingAddress.CountryRef.ThreeLetterIsoCode,
                                        Abbreviation = d.ContactRef.AddressBookRef.BillingAddress.StateProvinceRef.Abbreviation
                                    };

            string filter = string.Format("{0} == @0", searchType);
            object searchVal = searchText;

            if (searchType == VALUE_OF_ID)
                searchVal = Convert.ToInt32(searchText);
            
            return queryRequiredData.Where(filter, searchVal);            
        }

        public int AddDistributor(Distributor distributor)
        {
            _distributorRepository.Insert(distributor);
            _logService.AddLog(distributor.Id, "Created Now", "localhost", 1, DateTime.Now);

            return distributor.Id;
        }
        
        public List<DistributorInformationSourceType> GetDistributorInformationSourceTypeList()
        {
            string key = CACHE_INFORMATIONSOURCE_TYPE_LIST;

            return _cacheManager.Get(key, () =>
            {
                return _informationSourceTypeRepository.Table.ToList();
            });
        }

        public List<DistributorBusinessType> GetDistributorBusinessTypeList()
        {
            string key = CACHE_BUSINESS_TYPE_LIST;

            return _cacheManager.Get(key, () =>
            {
                return _businessTypeRepository.Table.ToList();
            });
        }

        public List<DistributorPayType> GetDistributorPayTypeList()
        {
            string key = CACHE_PAY_TYPE_LIST;

            return _cacheManager.Get(key, () =>
            {
                return _payTypeRepository.Table.ToList();
            });
        }


        public int GenerateDistributorCode()
        {
            int maxDistributorCode = (from d in _distributorRepository.Table
                                     select (int?) d.DistributorCode).Max() ?? 0;
            
            //int issystemmax = Convert.ToInt32(ConfigurationManager.AppSettings["SystemGenerated"]);

            //if( maxDistributorCode.HasValue)
            //    return maxDistributorCode.Value + 1;

            return maxDistributorCode + 1;

            /*
             * Original code
             * 
             * This is quite expensive
             * let us say we have 10000 records
             * we will download all the records and will be doing string comparison against all
             * also, if we store it as string we cannot get max value
             * for simplicity kept the data as Int
             * getting the max value from the database
             * we can discuss and change the implementation
             * 
            var query = _UserRepository.Table;
            int issystemmax = Convert.ToInt32((from n in query where n.IsSystemGenerated == true select n.CustomerCode).Max());
            if (issystemmax <= 0)
            {
                issystemmax = Convert.ToInt32(ConfigurationManager.AppSettings["SystemGenaerated"]);
            }
            else
            {
                issystemmax = issystemmax + 1;
            }
            List<User> allusers = (List<User>)(from users in query where users.IsSystemGenerated == false select users).ToList();

            foreach (User item in allusers)
            {
                if (item.CustomerCode == issystemmax.ToString())
                {
                    issystemmax++;
                }
            }
            return issystemmax;
            */
        }

        
        public Distributor GetDistributorById(int id)
        {
            return (from d in _distributorRepository.Table.Include(d => d.PersonalDetails)
                              where d.Id == id
                              select d).FirstOrDefault();
        }

        #endregion
    }
}
