-- =====================================================
-- Blog Posts: Service Business Management Category
-- CompanyId: 1
-- Date: 2025-02-13
-- =====================================================

-- Blog Post 1: Service Invoicing
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Invoicing for Service Businesses: Projects, Retainers, and Time-Based Billing' as "Title",
    'Service businesses bill differently—projects, hourly rates, retainers. Structure your invoices to get paid promptly and maintain clear client records.' as "ShortDescription",
    '<h2>Project-Based Billing</h2>
    <p>Quote a fixed price for a deliverable. Invoice on milestones or upfront. Clearly define scope to avoid scope creep and payment disputes.</p>
    
    <h3>Retainer Arrangements</h3>
    <p>Monthly fee for ongoing support. Generate recurring invoices automatically. Specify what''s included—hours, scope, response time. Review and renew periodically.</p>
    
    <h3>Time-Based Billing</h3>
    <p>Log hours against projects or clients. Convert to invoice at month-end or project completion. Use timesheets or integrated tracking. Transparent time reports build client trust.</p>
    
    <h3>SAC Codes for Services</h3>
    <p>Services use SAC codes, not HSN. Select the correct code for your service type. Consultancy, design, maintenance—each has a specific code. Ensures correct GST rate.</p>
    
    <h3>Conclusion</h3>
    <p>Service invoicing should be flexible. EquiBillBook supports project-based and recurring invoices with proper SAC classification for GST compliance.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Service Business Management' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'service invoicing, project billing, retainer, SAC code' as "Taglist",
    'Invoicing for Service Businesses: Projects, Retainers, and Time-Based Billing | EquiBillBook' as "MetaTitle",
    'Service businesses bill differently—projects, hourly rates, retainers. Structure your invoices to get paid promptly and maintain clear client records.' as "MetaDescription",
    'service invoicing, project billing, retainer, SAC code' as "MetaKeywords",
    'invoicing-service-businesses-projects-retainers-time-based-billing' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Service Business Management' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 2: Freelancer Finance
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Freelancer and Consultant Finance: Invoicing, Taxes, and Basic Bookkeeping' as "Title",
    'Solo professionals need simple systems. Invoice clients, track income, set aside tax, and maintain records—without hiring an accountant full-time.' as "ShortDescription",
    '<h2>Professional Invoicing</h2>
    <p>Clients expect clear, professional invoices. Include your details, client details, description of work, amount, payment terms, and bank details. GST registration if applicable.</p>
    
    <h3>Income Tracking</h3>
    <p>Log every payment received. Categorize by client or project. Running total of income helps with cash flow and tax planning. Simple spreadsheet or basic software works.</p>
    
    <h3>Tax Set-Aside</h3>
    <p>If you''re liable for GST or income tax, set aside a percentage of each payment. 18-25% is a rough buffer for many freelancers. Adjust based on your effective rate. Separate account works best.</p>
    
    <h3>Expense Records</h3>
    <p>Keep receipts for business expenses—software, travel, equipment. Deductible under income tax and potentially claimable as input credit under GST. Organize by month for easy filing.</p>
    
    <h3>Conclusion</h3>
    <p>Freelancers need lightweight tools. EquiBillBook offers affordable plans for solopreneurs—invoicing, GST, and basic reporting without complexity.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Service Business Management' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'freelancer finance, consultant invoicing, solo professional' as "Taglist",
    'Freelancer and Consultant Finance: Invoicing, Taxes, and Basic Bookkeeping | EquiBillBook' as "MetaTitle",
    'Solo professionals need simple systems. Invoice clients, track income, set aside tax, and maintain records—without hiring an accountant full-time.' as "MetaDescription",
    'freelancer finance, consultant invoicing, solo professional' as "MetaKeywords",
    'freelancer-consultant-finance-invoicing-taxes-bookkeeping' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Service Business Management' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 3: Client Management
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Client Management for Service Businesses: From Lead to Repeat Work' as "Title",
    'Service businesses live on relationships. Track clients, project history, and communication. Turn one-off projects into retained relationships.' as "ShortDescription",
    '<h2>Client Database</h2>
    <p>Central record per client: contact details, GSTIN if B2B, payment terms, preferred communication. Link all invoices and projects to the client. One place to see full history.</p>
    
    <h3>Project History</h3>
    <p>What did you deliver? When? For how much? Reference past work when pitching new projects. Identify clients who might need ongoing support.</p>
    
    <h3>Follow-Up Cadence</h3>
    <p>Don''t disappear after project completion. Check in at agreed intervals. Share relevant updates or insights. Low-pressure touch keeps you top of mind for next assignment.</p>
    
    <h3>Upsell and Cross-Sell</h3>
    <p>Clients who trust you for one service may need others. Offer packaged services or retainer options. Proactive outreach beats waiting for them to call.</p>
    
    <h3>Conclusion</h3>
    <p>Client management doesn''t require fancy CRM. Your billing system can double as a client database—invoices, payments, and notes in one place. EquiBillBook helps track client-wise revenue and outstanding.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Service Business Management' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'client management, service business, client relationship' as "Taglist",
    'Client Management for Service Businesses: From Lead to Repeat Work | EquiBillBook' as "MetaTitle",
    'Service businesses live on relationships. Track clients, project history, and communication. Turn one-off projects into retained relationships.' as "MetaDescription",
    'client management, service business, client relationship' as "MetaKeywords",
    'client-management-service-businesses-lead-to-repeat-work' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Service Business Management' AND "CompanyId" = 1 AND "IsDeleted" = false);
