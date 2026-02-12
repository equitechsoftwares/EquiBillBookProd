-- =====================================================
-- Blog Posts: Tax Planning & Savings Category
-- CompanyId: 1
-- Date: 2025-02-13
-- =====================================================

-- Blog Post 1: Input Tax Credit
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Maximizing GST Input Tax Credit: Eligibility and Documentation' as "Title",
    'Claim every rupee of input credit you''re entitled to. Understand eligibility rules, blocked credits, and the documentation needed to support your claims.' as "ShortDescription",
    '<h2>Input Credit Basics</h2>
    <p>Input Tax Credit (ITC) lets you offset tax paid on purchases against tax collected on sales. Proper documentation and timely filing are essential to claim it.</p>
    
    <h3>Eligibility Conditions</h3>
    <p>To claim ITC, ensure:</p>
    <ul>
        <li>Goods or services are used for business purposes</li>
        <li>Supplier has filed GSTR-1 and tax appears in your GSTR-2B</li>
        <li>You hold a valid tax invoice or debit note</li>
        <li>Goods have been received (for goods) or service period has ended</li>
    </ul>
    
    <h3>Blocked Credits</h3>
    <p>Certain expenses do not qualify for ITC:</p>
    <ul>
        <li>Motor vehicles (except for specified business use)</li>
        <li>Food, health, and membership club expenses</li>
        <li>Personal consumption or gifts</li>
        <li>Construction of immovable property (except plant and machinery)</li>
    </ul>
    
    <h3>Documentation Best Practices</h3>
    <p>Maintain invoices with correct GSTIN, HSN, and tax breakdown. Reconcile monthly with GSTR-2B. Address mismatches with suppliers before the annual return deadline.</p>
    
    <h3>Conclusion</h3>
    <p>Leaving ITC unclaimed is like leaving money on the table. Good billing software helps track purchase invoices and flags reconciliation issues early.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Tax Planning & Savings' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'input tax credit, GST ITC, tax savings, GST compliance' as "Taglist",
    'Maximizing GST Input Tax Credit: Eligibility and Documentation | EquiBillBook' as "MetaTitle",
    'Claim every rupee of input credit you''re entitled to. Understand eligibility rules, blocked credits, and the documentation needed to support your claims.' as "MetaDescription",
    'input tax credit, GST ITC, tax savings, GST compliance' as "MetaKeywords",
    'maximizing-gst-input-tax-credit-eligibility-documentation' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Tax Planning & Savings' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 2: Composition Scheme
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'GST Composition Scheme: Is It Right for Your Business?' as "Title",
    'Compare composition scheme benefits and limitations. Learn turnover thresholds, tax rates, and switching considerations for small businesses.' as "ShortDescription",
    '<h2>Composition Scheme Overview</h2>
    <p>The composition scheme offers simplified compliance for small businesses with turnover up to ₹1.5 crore (₹75 lakh for some states). You pay a fixed percentage of turnover instead of detailed GST returns.</p>
    
    <h3>Benefits</h3>
    <ul>
        <li>Quarterly returns instead of monthly</li>
        <li>Reduced compliance burden</li>
        <li>Lower tax rates for eligible businesses</li>
        <li>No need to collect GST from customers (in most cases)</li>
    </ul>
    
    <h3>Limitations</h3>
    <ul>
        <li>No input tax credit on purchases</li>
        <li>Cannot supply inter-state</li>
        <li>Cannot sell through e-commerce platforms</li>
        <li>Cannot issue tax invoices (only bill of supply)</li>
    </ul>
    
    <h3>When to Choose</h3>
    <p>Composition works well when your margins are high and purchase costs are low. If you buy heavily from organized suppliers, regular scheme with ITC may be more beneficial. Run the numbers before deciding.</p>
    
    <h3>Conclusion</h3>
    <p>Composition is a trade-off between simplicity and input credit. As your business grows, evaluate whether switching to regular GST makes financial sense.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Tax Planning & Savings' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'GST composition scheme, small business GST, tax simplification' as "Taglist",
    'GST Composition Scheme: Is It Right for Your Business? | EquiBillBook' as "MetaTitle",
    'Compare composition scheme benefits and limitations. Learn turnover thresholds, tax rates, and switching considerations for small businesses.' as "MetaDescription",
    'GST composition scheme, small business GST, tax simplification' as "MetaKeywords",
    'gst-composition-scheme-right-for-your-business' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Tax Planning & Savings' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 3: Tax Deductions
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Business Expense Deductions: What You Can Claim Under GST' as "Title",
    'Identify deductible business expenses for GST and income tax. Learn how to document and categorize expenses to maximize legitimate tax savings.' as "ShortDescription",
    '<h2>Expense Documentation</h2>
    <p>Tax savings start with proper documentation. Every business expense that reduces your tax liability should be supported by valid vouchers and invoices.</p>
    
    <h3>GST-Eligible Business Expenses</h3>
    <p>Common expenses that typically qualify for input credit:</p>
    <ul>
        <li>Raw materials and packaging for manufacturing</li>
        <li>Office supplies and equipment</li>
        <li>Professional services (legal, accounting, consulting)</li>
        <li>Rent for business premises</li>
        <li>Telecommunications and utilities</li>
    </ul>
    
    <h3>Expenses That Don''t Qualify</h3>
    <p>Personal use, entertainment, and certain specified items are blocked. Ensure you don''t claim credit on these to avoid interest and penalties during assessment.</p>
    
    <h3>Year-Round Discipline</h3>
    <p>Maintain expense records as you go. Use accounting software to categorize transactions. At year-end, you''ll have clean data for tax filing and reduced audit risk.</p>
    
    <h3>Conclusion</h3>
    <p>Legitimate tax planning relies on documentation and categorization. EquiBillBook helps track expenses and generate reports that support your tax position.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Tax Planning & Savings' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'business expense deductions, tax savings, GST expenses' as "Taglist",
    'Business Expense Deductions: What You Can Claim Under GST | EquiBillBook' as "MetaTitle",
    'Identify deductible business expenses for GST and income tax. Learn how to document and categorize expenses to maximize legitimate tax savings.' as "MetaDescription",
    'business expense deductions, tax savings, GST expenses' as "MetaKeywords",
    'business-expense-deductions-what-you-can-claim-gst' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Tax Planning & Savings' AND "CompanyId" = 1 AND "IsDeleted" = false);
