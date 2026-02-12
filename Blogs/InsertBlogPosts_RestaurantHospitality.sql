-- =====================================================
-- Blog Posts: Restaurant & Hospitality Category
-- CompanyId: 1
-- Date: 2025-02-13
-- =====================================================

-- Blog Post 1: Restaurant Billing
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Restaurant Billing: Tables, Courses, and Split Bills Made Simple' as "Title",
    'Restaurant billing has unique needs—table management, course-wise ordering, split bills, and tips. Choose software that handles these without slowing service.' as "ShortDescription",
    '<h2>Table Management</h2>
    <p>Map tables to orders. Track which table has which order. Merge or split tables when guests move. Clear table when done for quick turnover.</p>
    
    <h3>Course-Wise Ordering</h3>
    <p>Starters, main course, desserts—orders flow in stages. System should support adding items to an open order. Kitchen display or print tickets by course. Bill when all courses are done.</p>
    
    <h3>Split Bills</h3>
    <p>Groups often want separate checks. Split by seat, by item, or equally. Software that does this quickly avoids awkward moments at the table.</p>
    
    <h3>GST for Restaurants</h3>
    <p>Restaurant GST has specific rules—rate depends on turnover and AC/non-AC. Ensure your billing applies correct rates. Item-wise or bill-wise breakdown as required.</p>
    
    <h3>Conclusion</h3>
    <p>Restaurant billing needs to be fast and flexible. EquiBillBook offers restaurant module with table management and course-wise billing.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Restaurant & Hospitality' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'restaurant billing, table management, split bill' as "Taglist",
    'Restaurant Billing: Tables, Courses, and Split Bills Made Simple | EquiBillBook' as "MetaTitle",
    'Restaurant billing has unique needs—table management, course-wise ordering, split bills, and tips. Choose software that handles these without slowing service.' as "MetaDescription",
    'restaurant billing, table management, split bill' as "MetaKeywords",
    'restaurant-billing-tables-courses-split-bills-simple' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Restaurant & Hospitality' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 2: Kitchen and Inventory
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Restaurant Inventory: From Recipe to Reorder' as "Title",
    'Restaurant inventory is complex—raw ingredients, recipes, and wastage. Link menu items to ingredients, track usage, and reorder before you run out.' as "ShortDescription",
    '<h2>Recipe-Based Inventory</h2>
    <p>Each menu item uses specific ingredients. Define recipes: 1 biryani = 200g rice, 150g chicken, etc. When you sell biryani, system deducts from raw material stock.</p>
    
    <h3>Wastage and Variance</h3>
    <p>Real usage rarely matches recipe exactly. Track wastage separately. Conduct periodic physical counts. Variance reports highlight theft, spillage, or recipe errors.</p>
    
    <h3>Reorder Points</h3>
    <p>Set minimum levels for critical items. Receive alerts when stock is low. Factor in supplier lead time. Don''t run out of staples during peak hours.</p>
    
    <h3>Costing</h3>
    <p>Recipe costing helps set menu prices. Know the cost of each dish. Ensure gross margin targets are met. Adjust portions or prices when costs change.</p>
    
    <h3>Conclusion</h3>
    <p>Restaurant margins are thin. Good inventory control protects profitability. EquiBillBook''s restaurant module supports recipe-based inventory and wastage tracking.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Restaurant & Hospitality' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'restaurant inventory, recipe costing, food cost' as "Taglist",
    'Restaurant Inventory: From Recipe to Reorder | EquiBillBook' as "MetaTitle",
    'Restaurant inventory is complex—raw ingredients, recipes, and wastage. Link menu items to ingredients, track usage, and reorder before you run out.' as "MetaDescription",
    'restaurant inventory, recipe costing, food cost' as "MetaKeywords",
    'restaurant-inventory-from-recipe-to-reorder' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Restaurant & Hospitality' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 3: Hotel and Lodging
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Billing for Small Hotels and Lodging: Room Charges and Extras' as "Title",
    'Hotels and guest houses bill rooms, extras, and taxes. Learn how to structure billing for check-in, check-out, and incidentals.' as "ShortDescription",
    '<h2>Room Billing</h2>
    <p>Charge by night, with check-in and check-out times. Handle early check-in, late check-out, and partial day charges. Clear rate cards—weekday vs. weekend, seasonal pricing.</p>
    
    <h3>Incidentals</h3>
    <p>Mini-bar, laundry, room service, phone—add to folio as incurred. Ability to post charges to room from different departments. Consolidate at check-out.</p>
    
    <h3>Tax Compliance</h3>
    <p>Hospitality has specific GST treatment. Room tariff under certain threshold may have different rate. Ensure correct application. Generate compliant invoices for corporate clients.</p>
    
    <h3>Folio Management</h3>
    <p>Running folio per guest. Pre-authorization or advance payment. Settle with cash, card, or UPI at checkout. Receipt with full breakdown.</p>
    
    <h3>Conclusion</h3>
    <p>Lodging billing needs flexibility for rooms and extras. Look for software that supports folio-style billing and GST compliance for hospitality.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Restaurant & Hospitality' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'hotel billing, lodging, hospitality billing' as "Taglist",
    'Billing for Small Hotels and Lodging: Room Charges and Extras | EquiBillBook' as "MetaTitle",
    'Hotels and guest houses bill rooms, extras, and taxes. Learn how to structure billing for check-in, check-out, and incidentals.' as "MetaDescription",
    'hotel billing, lodging, hospitality billing' as "MetaKeywords",
    'billing-small-hotels-lodging-room-charges-extras' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Restaurant & Hospitality' AND "CompanyId" = 1 AND "IsDeleted" = false);
