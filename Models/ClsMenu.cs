using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblMenu")]
    public class ClsMenu
    {
        [Key]
        public long MenuId { get; set; }
        public long ParentId { get; set; }
        public string Menu { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public int Sequence { get; set; }
        public string Icon { get; set; }
        public bool HasChildren { get; set; }
        public string MenuType { get; set; }
        public bool IsMenu { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsExport { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool IsQuickLink { get; set; }
        public string Class { get; set; }
        public long QuickLinkParentId { get; set; }
        public long HeaderId { get; set; }
    }

    public class ClsMenuVm
    {
        public long MenuId { get; set; }
        public long ParentId { get; set; }
        public string Menu { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public int Sequence { get; set; }
        public string Icon { get; set; }
        public bool HasChildren { get; set; }
        public string MenuType { get; set; }
        public bool IsMenu { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long RoleId { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsExport { get; set; }
        public long MenuPermissionId { get; set; }
        public List<ClsInnerMenu> InnerMenus { get; set; }
        public bool IsViewEnabled { get; set; }
        public bool IsAddEnabled { get; set; }
        public bool IsEditEnabled { get; set; }
        public bool IsDeleteEnabled { get; set; }
        public bool IsExportEnabled { get; set; }
        public long CompanyId { get; set; }
        public string Domain { get; set; }
        public string ParentMenu { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public bool IsQuickLink { get; set; }
        public string Class { get; set; }
        public long QuickLinkParentId { get; set; }
        public long HeaderId { get; set; }
        public string ReportType { get; set; }
        public int PageIndex { get; set; }
        public string UserType { get; set; }
        public long BranchId { get; set; }
        public int PageSize { get; set; }
    }

    public class ClsInnerMenu
    {
        public long MenuId { get; set; }
        public long ParentId { get; set; }
        public string Menu { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public int Sequence { get; set; }
        public string Icon { get; set; }
        public bool HasChildren { get; set; }
        public string MenuType { get; set; }
        public bool IsMenu { get; set; }
        public bool IsView { get; set; }
        public bool IsAdd { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsExport { get; set; }
        public bool IsViewEnabled { get; set; }
        public bool IsAddEnabled { get; set; }
        public bool IsEditEnabled { get; set; }
        public bool IsDeleteEnabled { get; set; }
        public bool IsExportEnabled { get; set; }
        public long MenuPermissionId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsQuickLink { get; set; }
        public string Class { get; set; }
        public long QuickLinkParentId { get; set; }
        public long HeaderId { get; set; }
    }

}