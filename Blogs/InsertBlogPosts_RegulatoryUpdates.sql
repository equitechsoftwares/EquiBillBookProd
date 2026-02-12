-- =====================================================
-- Blog Posts: Regulatory Updates Category
-- CompanyId: 1
-- Date: 2025-02-13
-- =====================================================

-- Blog Post 1: GST Rate Changes
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Staying Updated on GST Rate Changes: How to Adapt Quickly' as "Title",
    'GST rates change through notifications and council decisions. Learn how to track updates, adjust your systems, and communicate changes to customers.' as "ShortDescription",
    '<h2>Where Updates Come From</h2>
    <p>GST Council recommends changes. CBIC issues notifications. Rate changes, new exemptions, and clarifications are published on the GST portal and official gazette.</p>
    
    <h3>Staying Informed</h3>
    <ul>
        <li>GST portal: Check notifications and circulars</li>
        <li>CBIC website: Official communications</li>
        <li>Industry associations: Often summarize changes</li>
        <li>Your software vendor: Should update rate masters</li>
    </ul>
    
    <h3>Implementation Checklist</h3>
    <p>When a rate changes: Update product/service master with new rate. Effective date matters—invoices before that date use old rate. Test a few invoices. Communicate to customers if prices change.</p>
    
    <h3>Transitional Invoices</h3>
    <p>For orders placed before rate change but delivered after, transitional rules may apply. Document and apply correctly. Consult your CA for complex cases.</p>
    
    <h3>Conclusion</h3>
    <p>Rate changes are manageable when you have a process. EquiBillBook maintains updated tax rates so your invoices stay compliant with minimal effort.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Regulatory Updates' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'GST rate changes, GST updates, tax compliance' as "Taglist",
    'Staying Updated on GST Rate Changes: How to Adapt Quickly | EquiBillBook' as "MetaTitle",
    'GST rates change through notifications and council decisions. Learn how to track updates, adjust your systems, and communicate changes to customers.' as "MetaDescription",
    'GST rate changes, GST updates, tax compliance' as "MetaKeywords",
    'staying-updated-gst-rate-changes-how-to-adapt-quickly' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Regulatory Updates' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 2: E-Invoicing Threshold
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'E-Invoicing Threshold Changes: Are You Ready When Your Turn Comes?' as "Title",
    'E-invoicing applicability has been phased by turnover. Understand current thresholds, who''s covered, and how to prepare before you cross the limit.' as "ShortDescription",
    '<h2>Phased Rollout</h2>
    <p>E-invoicing was introduced for large businesses and has been extended to smaller turnovers. Check current notification for your aggregate turnover bracket.</p>
    
    <h3>Aggregate Turnover</h3>
    <p>Threshold is based on PAN-level turnover—all GSTINs under same PAN. Even if one business vertical is below threshold, combined turnover may bring you in scope.</p>
    
    <h3>Preparation Steps</h3>
    <ul>
        <li>Ensure billing software supports e-invoicing</li>
        <li>Test generation and IRP submission</li>
        <li>Train staff on the workflow</li>
        <li>Plan for B2C invoices if required</li>
    </ul>
    
    <h3>Exemptions</h3>
    <p>Certain sectors may have exemptions or extended timelines. Check notifications for your industry. Don''t assume—verify.</p>
    
    <h3>Conclusion</h3>
    <p>Early preparation avoids last-minute scramble. EquiBillBook keeps abreast of e-invoicing requirements so you''re ready when the mandate applies to you.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Regulatory Updates' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'e-invoicing threshold, e-invoicing mandate, IRP' as "Taglist",
    'E-Invoicing Threshold Changes: Are You Ready When Your Turn Comes? | EquiBillBook' as "MetaTitle",
    'E-invoicing applicability has been phased by turnover. Understand current thresholds, who''s covered, and how to prepare before you cross the limit.' as "MetaDescription",
    'e-invoicing threshold, e-invoicing mandate, IRP' as "MetaKeywords",
    'e-invoicing-threshold-changes-ready-when-your-turn-comes' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Regulatory Updates' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 3: Return Filing Updates
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'GST Return Filing Updates: New Forms, Deadlines, and Simplifications' as "Title",
    'GST return structure has evolved. Stay current on GSTR-1, GSTR-3B, annual return, and any new simplified schemes or form changes.' as "ShortDescription",
    '<h2>Current Return Structure</h2>
    <p>GSTR-1: Outward supplies, filed monthly or quarterly based on turnover. GSTR-3B: Summary return with tax payment, monthly. Annual return (GSTR-9) for those above threshold. Reconciliation statements where applicable.</p>
    
    <h3>Quarterly Filing Option</h3>
    <p>Small businesses under turnover limit can opt for QRMP—quarterly GSTR-1 and GSTR-3B with monthly tax payment. Reduces filing frequency. Check eligibility and opt-in process.</p>
    
    <h3>Deadline Awareness</h3>
    <p>Deadlines vary by return type and sometimes by staggered rollout. Mark calendar. Late filing attracts interest and late fee. Set reminders well in advance.</p>
    
    <h3>Reconciliation</h3>
    <p>GSTR-2B (auto-drafted from supplier filings) helps reconcile input credit. Match purchase register with GSTR-2B. Discrepancies need resolution—contact supplier or claim with caution.</p>
    
    <h3>Conclusion</h3>
    <p>Return filing is core compliance. Stay updated on form changes and deadlines. EquiBillBook generates reports that support GSTR-1 and GSTR-3B preparation.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Regulatory Updates' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'GST return filing, GSTR-1, GSTR-3B, QRMP' as "Taglist",
    'GST Return Filing Updates: New Forms, Deadlines, and Simplifications | EquiBillBook' as "MetaTitle",
    'GST return structure has evolved. Stay current on GSTR-1, GSTR-3B, annual return, and any new simplified schemes or form changes.' as "MetaDescription",
    'GST return filing, GSTR-1, GSTR-3B, QRMP' as "MetaKeywords",
    'gst-return-filing-updates-new-forms-deadlines-simplifications' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Regulatory Updates' AND "CompanyId" = 1 AND "IsDeleted" = false);
