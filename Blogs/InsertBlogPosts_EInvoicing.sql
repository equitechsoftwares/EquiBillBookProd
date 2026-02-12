-- =====================================================
-- Blog Posts: E-Invoicing & Digital Payments Category
-- CompanyId: 1
-- Date: 2025-02-13
-- =====================================================

-- Blog Post 1: E-Invoicing Guide
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'E-Invoicing Under GST: Complete Guide for Indian Businesses' as "Title",
    'E-invoicing is mandatory for many businesses. Understand the process, IRP integration, and how to generate compliant e-invoices without hassle.' as "ShortDescription",
    '<h2>What is E-Invoicing?</h2>
    <p>E-invoicing means generating invoices in a standardized format and registering them with the Invoice Registration Portal (IRP) before delivery to the customer. The IRP returns a unique Invoice Reference Number (IRN) and QR code.</p>
    
    <h3>Who Must Comply?</h3>
    <p>Thresholds have been lowering over time. Check current rules based on your turnover. Even if not mandatory yet, early adoption prepares you for when it becomes applicable.</p>
    
    <h3>Process Flow</h3>
    <ul>
        <li>Create invoice in your system with required fields</li>
        <li>Generate JSON in the specified schema</li>
        <li>Submit to IRP via API</li>
        <li>Receive IRN and signed QR code</li>
        <li>Add QR to invoice and share with customer</li>
    </ul>
    
    <h3>Software Integration</h3>
    <p>Manual submission is impractical. Your billing software should integrate with the IRP. One-click e-invoice generation from within your normal workflow keeps compliance seamless.</p>
    
    <h3>Conclusion</h3>
    <p>E-invoicing streamlines compliance and reduces manual intervention. Choose software that supports e-invoicing so you''re ready when it applies to you.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'E-Invoicing & Digital Payments' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'e-invoicing, GST e-invoice, IRP, Invoice Reference Number' as "Taglist",
    'E-Invoicing Under GST: Complete Guide for Indian Businesses | EquiBillBook' as "MetaTitle",
    'E-invoicing is mandatory for many businesses. Understand the process, IRP integration, and how to generate compliant e-invoices without hassle.' as "MetaDescription",
    'e-invoicing, GST e-invoice, IRP, Invoice Reference Number' as "MetaKeywords",
    'e-invoicing-under-gst-complete-guide-indian-businesses' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'E-Invoicing & Digital Payments' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 2: Digital Payment Acceptance
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Accepting Digital Payments: UPI, Cards, and Payment Links' as "Title",
    'Offer customers multiple ways to pay. Compare UPI, card terminals, and payment links—and how to integrate them with your billing for faster collections.' as "ShortDescription",
    '<h2>Payment Options for SMEs</h2>
    <p>Customers expect convenience. The more payment options you offer, the fewer excuses for delayed payment.</p>
    
    <h3>UPI</h3>
    <p>QR codes and UPI IDs are low-cost to set up. Customers scan and pay. Funds settle quickly. Integrate with your bank or use a payment aggregator.</p>
    
    <h3>Card Terminals</h3>
    <p>POS machines accept debit and credit cards. Useful for in-person retail and restaurants. Rental or transaction-based pricing. Ensure EMV compliance.</p>
    
    <h3>Payment Links</h3>
    <p>Send a link via WhatsApp or email. Customer pays without visiting your office. Ideal for B2B, remote sales, and recurring payments. Link generation can be tied to specific invoices.</p>
    
    <h3>Reconciliation</h3>
    <p>Multiple channels mean multiple settlement reports. Software that matches payments to invoices by amount, date, or reference saves hours of manual work.</p>
    
    <h3>Conclusion</h3>
    <p>Digital payment acceptance is table stakes. EquiBillBook supports invoice sharing via WhatsApp and integration with payment flows for smoother collections.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'E-Invoicing & Digital Payments' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'digital payments, UPI, payment links, SME payments' as "Taglist",
    'Accepting Digital Payments: UPI, Cards, and Payment Links | EquiBillBook' as "MetaTitle",
    'Offer customers multiple ways to pay. Compare UPI, card terminals, and payment links—and how to integrate them with your billing for faster collections.' as "MetaDescription",
    'digital payments, UPI, payment links, SME payments' as "MetaKeywords",
    'accepting-digital-payments-upi-cards-payment-links' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'E-Invoicing & Digital Payments' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 3: Payment Reconciliation
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Payment Reconciliation: Match Invoices to Payments Without the Headache' as "Title",
    'When payments arrive from multiple channels, reconciliation gets messy. Learn best practices for matching, partial payments, and handling discrepancies.' as "ShortDescription",
    '<h2>The Reconciliation Challenge</h2>
    <p>You send 50 invoices. Payments arrive via UPI, bank transfer, cheque, and card. Which payment covers which invoice? Manual matching wastes time and invites errors.</p>
    
    <h3>Clear Payment References</h3>
    <p>Ask customers to include invoice number in payment narration or reference. Train your team to mention it when sharing payment links. Reduces ambiguity dramatically.</p>
    
    <h3>Partial Payments</h3>
    <p>Customers sometimes pay in installments. Record partial payments against the invoice. Update outstanding balance. Track fully paid vs. partially paid for follow-up.</p>
    
    <h3>Automation Where Possible</h3>
    <p>If your software connects to payment gateways or bank feeds, use auto-matching. Rules based on amount, date, or customer can suggest matches. Review and confirm.</p>
    
    <h3>Exception Handling</h3>
    <p>Unidentified payments need a process. Contact customer, check bank statement, reconcile. Don''t leave them in a suspense account indefinitely.</p>
    
    <h3>Conclusion</h3>
    <p>Efficient reconciliation improves cash flow visibility. EquiBillBook helps track payments against invoices and maintain clear receivables aging.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'E-Invoicing & Digital Payments' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'payment reconciliation, invoice matching, receivables' as "Taglist",
    'Payment Reconciliation: Match Invoices to Payments Without the Headache | EquiBillBook' as "MetaTitle",
    'When payments arrive from multiple channels, reconciliation gets messy. Learn best practices for matching, partial payments, and handling discrepancies.' as "MetaDescription",
    'payment reconciliation, invoice matching, receivables' as "MetaKeywords",
    'payment-reconciliation-match-invoices-payments-without-headache' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'E-Invoicing & Digital Payments' AND "CompanyId" = 1 AND "IsDeleted" = false);
