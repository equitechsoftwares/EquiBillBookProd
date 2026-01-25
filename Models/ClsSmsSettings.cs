using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblSmsSettings")]
    public class ClsSmsSettings
    {
        [Key]
        public long SmsSettingsId { get; set; }
        public int SmsService { get; set; }
        public int RequestMethod { get; set; }
        public string Url { get; set; }
        public string SendToParameterName { get; set; }
        public string MessageParameterName { get; set; }
        public string Header1Key { get; set; }
        public string Header1Value { get; set; }
        public string Header2Key { get; set; }
        public string Header2Value { get; set; }
        public string Header3Key { get; set; }
        public string Header3Value { get; set; }
        public string Header4Key { get; set; }
        public string Header4Value { get; set; }
        public string Parameter1Key { get; set; }
        public string Parameter1Value { get; set; }
        public string Parameter2Key { get; set; }
        public string Parameter2Value { get; set; }
        public string Parameter3Key { get; set; }
        public string Parameter3Value { get; set; }
        public string Parameter4Key { get; set; }
        public string Parameter4Value { get; set; }
        public string Parameter5Key { get; set; }
        public string Parameter5Value { get; set; }
        public string Parameter6Key { get; set; }
        public string Parameter6Value { get; set; }
        public string Parameter7Key { get; set; }
        public string Parameter7Value { get; set; }
        public string Parameter8Key { get; set; }
        public string Parameter8Value { get; set; }
        public string Parameter9Key { get; set; }
        public string Parameter9Value { get; set; }
        public string Parameter10Key { get; set; }
        public string Parameter10Value { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string TwilioAccountSID { get; set; }
        public string TwilioAuthToken { get; set; }
        public string TwilioFrom { get; set; }
        public string NexmoApiKey { get; set; }
        public string NexmoApiSecret { get; set; }
        public string NexmoFrom { get; set; }
        public bool EnableCountryCode { get; set; }
        public string TestMobileNo { get; set; }
        public bool IsDefault { get; set; }
        public string SaveAs { get; set; }
    }

    public class ClsSmsSettingsVm
    {
        public long SmsSettingsId { get; set; }
        public int SmsService { get; set; }
        public int RequestMethod { get; set; }
        public string Url { get; set; }
        public string SendToParameterName { get; set; }
        public string MessageParameterName { get; set; }
        public string Header1Key { get; set; }
        public string Header1Value { get; set; }
        public string Header2Key { get; set; }
        public string Header2Value { get; set; }
        public string Header3Key { get; set; }
        public string Header3Value { get; set; }
        public string Header4Key { get; set; }
        public string Header4Value { get; set; }
        public string Parameter1Key { get; set; }
        public string Parameter1Value { get; set; }
        public string Parameter2Key { get; set; }
        public string Parameter2Value { get; set; }
        public string Parameter3Key { get; set; }
        public string Parameter3Value { get; set; }
        public string Parameter4Key { get; set; }
        public string Parameter4Value { get; set; }
        public string Parameter5Key { get; set; }
        public string Parameter5Value { get; set; }
        public string Parameter6Key { get; set; }
        public string Parameter6Value { get; set; }
        public string Parameter7Key { get; set; }
        public string Parameter7Value { get; set; }
        public string Parameter8Key { get; set; }
        public string Parameter8Value { get; set; }
        public string Parameter9Key { get; set; }
        public string Parameter9Value { get; set; }
        public string Parameter10Key { get; set; }
        public string Parameter10Value { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string TwilioAccountSID { get; set; }
        public string TwilioAuthToken { get; set; }
        public string TwilioFrom { get; set; }
        public string NexmoApiKey { get; set; }
        public string NexmoApiSecret { get; set; }
        public string NexmoFrom { get; set; }
        public bool EnableCountryCode { get; set; }
        public string TestMobileNo { get; set; }
        public bool IsDefault { get; set; }
        public string SaveAs { get; set; }
        public string DialingCode { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }

}