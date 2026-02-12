-- =====================================================
-- Blog Posts: Success Stories & Case Studies Category
-- CompanyId: 1
-- Date: 2025-02-13
-- =====================================================

-- Blog Post 1: Retail Store Turnaround
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'From Paper Ledgers to Digital: A Retailer''s Billing Transformation' as "Title",
    'See how a family-run retail store cut billing errors and saved 10 hours per week by switching to GST billing software and digital invoicing.' as "ShortDescription",
    '<h2>The Challenge</h2>
    <p>A 15-year-old retail store in a tier-2 city was using manual ledgers and handwritten bills. GST compliance was a monthly nightmare—errors, late filings, and constant accountant visits.</p>
    
    <h3>What Changed</h3>
    <p>The owner adopted cloud-based billing software. Invoices became professional and GST-compliant. Stock levels updated automatically with each sale. Reports were available at a click.</p>
    
    <h3>Measurable Outcomes</h3>
    <ul>
        <li>10 hours saved weekly on billing and reconciliation</li>
        <li>Zero late filing penalties in 12 months</li>
        <li>Better supplier negotiations with accurate purchase history</li>
        <li>Faster customer checkout with barcode scanning</li>
    </ul>
    
    <h3>Key Takeaway</h3>
    <p>Transition doesn''t require tech expertise. With guided setup and local support, even traditional businesses can modernize billing in weeks.</p>
    
    <h3>Conclusion</h3>
    <p>Digital billing isn''t just for tech-savvy businesses. EquiBillBook is designed for retailers who want simplicity without sacrificing compliance or reporting.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Success Stories & Case Studies' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'retail success story, billing transformation, SME case study' as "Taglist",
    'From Paper Ledgers to Digital: A Retailer''s Billing Transformation | EquiBillBook' as "MetaTitle",
    'See how a family-run retail store cut billing errors and saved 10 hours per week by switching to GST billing software and digital invoicing.' as "MetaDescription",
    'retail success story, billing transformation, SME case study' as "MetaKeywords",
    'paper-ledgers-to-digital-retailer-billing-transformation' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Success Stories & Case Studies' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 2: Multi-Branch Restaurant
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'How a Quick-Service Restaurant Chain Unified Billing Across 4 Outlets' as "Title",
    'A QSR chain struggled with fragmented billing and inventory. Discover how consolidated software helped them achieve real-time visibility and consistent compliance.' as "ShortDescription",
    '<h2>The Problem</h2>
    <p>Four outlets, four different billing systems. Head office spent days consolidating data. Inventory was often wrong—overstocking at one outlet, stockouts at another.</p>
    
    <h3>The Solution</h3>
    <p>Multi-location billing software with central dashboard. Each outlet logged sales and stock movements. Real-time sync gave management a unified view.</p>
    
    <h3>Results</h3>
    <ul>
        <li>Consolidated reports in minutes instead of days</li>
        <li>Inter-outlet stock transfers became trackable</li>
        <li>GST returns filed from one place for all outlets</li>
        <li>Outlet-wise performance comparison enabled better decisions</li>
    </ul>
    
    <h3>Lesson Learned</h3>
    <p>Scaling locations without unified systems creates chaos. Invest in multi-branch software before opening your next outlet.</p>
    
    <h3>Conclusion</h3>
    <p>Chain businesses need location-wise data with consolidated control. EquiBillBook''s multi-branch features support restaurant and retail chains.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Success Stories & Case Studies' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'restaurant chain, multi-branch, QSR case study' as "Taglist",
    'How a Quick-Service Restaurant Chain Unified Billing Across 4 Outlets | EquiBillBook' as "MetaTitle",
    'A QSR chain struggled with fragmented billing and inventory. Discover how consolidated software helped them achieve real-time visibility and consistent compliance.' as "MetaDescription",
    'restaurant chain, multi-branch, QSR case study' as "MetaKeywords",
    'quick-service-restaurant-chain-unified-billing-4-outlets' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Success Stories & Case Studies' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 3: Wholesale Distributor
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Wholesale Distributor Cuts Order Processing Time by 60%' as "Title",
    'High order volume and complex pricing made manual processing unsustainable. Learn how automation and smart invoicing changed the game for a distributor.' as "ShortDescription",
    '<h2>Before Automation</h2>
    <p>A distributor handling 200+ orders daily used Excel and manual data entry. Dedicated staff spent hours creating invoices, applying customer-specific prices, and tracking payments.</p>
    
    <h3>Transformation Steps</h3>
    <ul>
        <li>Customer and product masters with pre-defined pricing</li>
        <li>Bulk invoice generation from order data</li>
        <li>Automated payment reminders and aging reports</li>
        <li>E-way bill generation linked to invoices</li>
    </ul>
    
    <h3>Impact</h3>
    <p>Order-to-invoice time dropped by 60%. Fewer errors meant fewer customer disputes. Staff shifted from data entry to relationship management and collections.</p>
    
    <h3>Conclusion</h3>
    <p>Volume businesses need automation. EquiBillBook supports bulk operations, custom pricing, and e-way bills for distributors and wholesalers.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Success Stories & Case Studies' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'wholesale distributor, order automation, case study' as "Taglist",
    'Wholesale Distributor Cuts Order Processing Time by 60% | EquiBillBook' as "MetaTitle",
    'High order volume and complex pricing made manual processing unsustainable. Learn how automation and smart invoicing changed the game for a distributor.' as "MetaDescription",
    'wholesale distributor, order automation, case study' as "MetaKeywords",
    'wholesale-distributor-cuts-order-processing-time-60-percent' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Success Stories & Case Studies' AND "CompanyId" = 1 AND "IsDeleted" = false);
