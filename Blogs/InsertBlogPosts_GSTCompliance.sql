-- =====================================================
-- Blog Posts: GST Compliance & Updates Category
-- CompanyId: 1
-- Date: 2025-02-13
-- =====================================================

-- Blog Post 1: GST Filing Basics
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'GST Filing Checklist: Essential Steps for Timely Returns' as "Title",
    'Navigate GST filing with confidence. A practical checklist covering invoice preparation, input credit reconciliation, return filing deadlines, and common compliance pitfalls.' as "ShortDescription",
    '<h2>Your GST Filing Roadmap</h2>
    <p>Meeting GST filing deadlines avoids penalties and keeps your business compliant. This guide walks you through the critical steps for a smooth filing experience.</p>
    
    <h3>Pre-Filing Preparation</h3>
    <p>Before logging into the GST portal, ensure these items are ready:</p>
    <ul>
        <li>Consolidated sales data from all business locations</li>
        <li>Purchase invoices with valid GSTIN of suppliers</li>
        <li>Bank statements for reconciliation</li>
        <li>E-way bill summaries if applicable</li>
        <li>Reversal entries for non-business usage</li>
    </ul>
    
    <h3>Invoice Verification</h3>
    <p>Cross-check that all invoices meet GST requirements:</p>
    <ul>
        <li>Correct HSN codes for goods or SAC for services</li>
        <li>Place of supply details for inter-state transactions</li>
        <li>Tax rate alignment with current GST slabs</li>
        <li>Customer GSTIN on B2B invoices</li>
    </ul>
    
    <h3>Input Tax Credit Reconciliation</h3>
    <p>Match your books with GSTR-2B to claim valid credits. Discrepancies in supplier details can delay or deny credit. Regular reconciliation throughout the month prevents last-minute rush.</p>
    
    <h3>Common Filing Errors</h3>
    <p>Watch out for these frequent mistakes:</p>
    <ul>
        <li>Mismatched turnover between GSTR-1 and GSTR-3B</li>
        <li>Incorrect tax rate selection causing under or overpayment</li>
        <li>Missing reverse charge entries</li>
        <li>Forgotten nil return filing for inactive months</li>
    </ul>
    
    <h3>Conclusion</h3>
    <p>Using GST billing software can automate much of this workflow. EquiBillBook simplifies invoice generation, tax calculations, and report preparation so you can focus on business growth while staying compliant.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'GST Compliance & Updates' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'GST filing, GST compliance, GST returns, input tax credit, GSTR filing' as "Taglist",
    'GST Filing Checklist: Essential Steps for Timely Returns | EquiBillBook' as "MetaTitle",
    'Navigate GST filing with confidence. A practical checklist covering invoice preparation, input credit reconciliation, return filing deadlines, and common compliance pitfalls.' as "MetaDescription",
    'GST filing, GST compliance, GST returns, input tax credit, GSTR filing, GST checklist' as "MetaKeywords",
    'gst-filing-checklist-essential-steps-timely-returns' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'GST Compliance & Updates' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 2: E-Way Bill Compliance
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'E-Way Bill Rules: When You Need One and How to Generate' as "Title",
    'Understand e-way bill requirements for goods movement in India. Learn thresholds, exemption scenarios, and how to generate e-way bills correctly for your business.' as "ShortDescription",
    '<h2>E-Way Bill Essentials</h2>
    <p>An e-way bill is an electronic document required for moving goods worth more than ₹50,000. Getting it right ensures smooth logistics and avoids penalties at checkpoints.</p>
    
    <h3>When Do You Need an E-Way Bill?</h3>
    <p>Generate an e-way bill when:</p>
    <ul>
        <li>Goods value exceeds ₹50,000 (single invoice or consignment)</li>
        <li>Transporting goods across state borders</li>
        <li>Moving goods within a state for specified distances</li>
        <li>Goods are in transit and value crosses threshold</li>
    </ul>
    
    <h3>Exemptions to Know</h3>
    <p>Certain goods and scenarios are exempt:</p>
    <ul>
        <li>Goods listed in exemption schedule</li>
        <li>Movement by non-motorized conveyance</li>
        <li>Goods transported under customs bond</li>
        <li>Specified commodities like newspapers, milk, etc.</li>
    </ul>
    
    <h3>Generation Best Practices</h3>
    <p>Generate e-way bills before goods dispatch. Include accurate transit details, vehicle number, and document references. Update or cancel if consignment details change. Integrate e-way bill generation with your billing software for seamless workflow.</p>
    
    <h3>Common Mistakes</h3>
    <p>Avoid these errors:</p>
    <ul>
        <li>Wrong vehicle number or document type</li>
        <li>Missing or incorrect HSN codes</li>
        <li>Continuing with expired e-way bill</li>
        <li>Not cancelling when goods are not dispatched</li>
    </ul>
    
    <h3>Conclusion</h3>
    <p>E-way bill compliance is straightforward when integrated into your billing process. Modern GST software can generate e-way bills directly from invoices, reducing manual entry and errors.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'GST Compliance & Updates' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'e-way bill, GST e-way bill, goods movement, transport compliance' as "Taglist",
    'E-Way Bill Rules: When You Need One and How to Generate | EquiBillBook' as "MetaTitle",
    'Understand e-way bill requirements for goods movement in India. Learn thresholds, exemption scenarios, and how to generate e-way bills correctly for your business.' as "MetaDescription",
    'e-way bill, GST e-way bill, goods movement, transport compliance, e-way bill generation' as "MetaKeywords",
    'e-way-bill-rules-when-you-need-one-how-to-generate' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'GST Compliance & Updates' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 3: HSN and SAC Codes
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'HSN and SAC Codes: Choosing the Right Tax Classification for Your Products' as "Title",
    'Master HSN codes for goods and SAC codes for services. Learn how correct classification affects your GST rates and compliance documentation.' as "ShortDescription",
    '<h2>Tax Classification Basics</h2>
    <p>HSN (Harmonized System of Nomenclature) codes classify goods; SAC (Services Accounting Code) classifies services. Using the correct code ensures proper tax application and smooth audits.</p>
    
    <h3>Why Classification Matters</h3>
    <p>Wrong codes lead to:</p>
    <ul>
        <li>Incorrect tax rates on invoices</li>
        <li>Input credit mismatches with suppliers</li>
        <li>Scrutiny during GST assessment</li>
        <li>Penalties for misclassification</li>
    </ul>
    
    <h3>Finding the Right HSN Code</h3>
    <p>Start with the broad category, then narrow down. Use 4-digit codes for turnover under ₹5 crore; 6-digit for higher turnover. Refer to the GST rate schedule and CBIC notifications for updates.</p>
    
    <h3>SAC for Service Businesses</h3>
    <p>Services have 6-digit SAC codes. Common codes include consulting (9983), repair (9987), and rental (9971). Match your service description to the closest SAC in the GST schedule.</p>
    
    <h3>Updates and Changes</h3>
    <p>GST rates and classifications change periodically. Subscribe to official updates and ensure your billing software supports easy code updates. Document your classification rationale for audit purposes.</p>
    
    <h3>Conclusion</h3>
    <p>Accurate HSN and SAC codes protect your business from compliance issues. EquiBillBook maintains updated tax code libraries so you can invoice with confidence.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'GST Compliance & Updates' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'HSN code, SAC code, GST classification, tax rates' as "Taglist",
    'HSN and SAC Codes: Choosing the Right Tax Classification for Your Products | EquiBillBook' as "MetaTitle",
    'Master HSN codes for goods and SAC codes for services. Learn how correct classification affects your GST rates and compliance documentation.' as "MetaDescription",
    'HSN code, SAC code, GST classification, tax rates, product classification' as "MetaKeywords",
    'hsn-sac-codes-choosing-right-tax-classification' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'GST Compliance & Updates' AND "CompanyId" = 1 AND "IsDeleted" = false);
