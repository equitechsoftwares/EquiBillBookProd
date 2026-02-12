-- =====================================================
-- Blog Posts: Inventory Management Insights Category
-- CompanyId: 1
-- Date: 2025-02-13
-- =====================================================

-- Blog Post 1: Stock Optimization
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Stock Optimization: Balancing Availability with Capital Efficiency' as "Title",
    'Hold the right amount of stock—not too much, not too little. Learn reorder points, safety stock, and how to reduce dead inventory without losing sales.' as "ShortDescription",
    '<h2>The Stock Balancing Act</h2>
    <p>Excess inventory ties up capital; stockouts lose sales. Finding the sweet spot requires data, not gut feeling.</p>
    
    <h3>Reorder Point Calculation</h3>
    <p>Set reorder points based on:</p>
    <ul>
        <li>Average daily consumption from historical data</li>
        <li>Lead time from supplier (in days)</li>
        <li>Buffer for variability in demand or supply</li>
    </ul>
    <p>Formula: Reorder Point = (Daily Usage × Lead Time) + Safety Stock</p>
    
    <h3>Identifying Slow-Moving Items</h3>
    <p>Run aging reports on inventory. Items with zero movement for 90+ days need attention. Consider clearance pricing, bundling, or discontinuing to free up space and capital.</p>
    
    <h3>Technology for Visibility</h3>
    <p>Real-time stock levels across locations prevent overselling and enable accurate forecasting. Software that integrates billing with inventory gives you a single source of truth.</p>
    
    <h3>Conclusion</h3>
    <p>Optimized stock levels improve cash flow and reduce storage costs. EquiBillBook''s inventory module tracks stock across locations and alerts you when reorder points are reached.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Inventory Management Insights' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'inventory optimization, stock management, reorder point, safety stock' as "Taglist",
    'Stock Optimization: Balancing Availability with Capital Efficiency | EquiBillBook' as "MetaTitle",
    'Hold the right amount of stock—not too much, not too little. Learn reorder points, safety stock, and how to reduce dead inventory without losing sales.' as "MetaDescription",
    'inventory optimization, stock management, reorder point, safety stock' as "MetaKeywords",
    'stock-optimization-balancing-availability-capital-efficiency' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Inventory Management Insights' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 2: ABC Analysis
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'ABC Analysis for Inventory: Prioritize What Matters Most' as "Title",
    'Classify your inventory by value and movement. ABC analysis helps you focus control efforts on high-impact items and simplify management for the rest.' as "ShortDescription",
    '<h2>What is ABC Analysis?</h2>
    <p>ABC analysis divides inventory into three categories based on annual consumption value (units × price × frequency). A few items typically drive most of your value.</p>
    
    <h3>Category Definitions</h3>
    <ul>
        <li><strong>A Items:</strong> High value, low quantity—tight control, frequent counts, priority reordering</li>
        <li><strong>B Items:</strong> Moderate value—standard controls and periodic review</li>
        <li><strong>C Items:</strong> Low value, high quantity—minimal control, bulk ordering</li>
    </ul>
    
    <h3>How to Implement</h3>
    <p>Export sales and inventory data. Calculate consumption value per SKU. Sort descending. Typically, top 20% of items account for 80% of value—those are your A items.</p>
    
    <h3>Applying the Framework</h3>
    <p>Use different strategies per category. A items get detailed forecasting and minimal stockouts. C items can use simple min-max rules. This approach reduces effort while improving outcomes.</p>
    
    <h3>Conclusion</h3>
    <p>ABC analysis brings focus to inventory management. Reporting tools in billing software can help you run this analysis periodically and adjust strategies as demand patterns shift.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Inventory Management Insights' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'ABC analysis, inventory classification, stock prioritization' as "Taglist",
    'ABC Analysis for Inventory: Prioritize What Matters Most | EquiBillBook' as "MetaTitle",
    'Classify your inventory by value and movement. ABC analysis helps you focus control efforts on high-impact items and simplify management for the rest.' as "MetaDescription",
    'ABC analysis, inventory classification, stock prioritization' as "MetaKeywords",
    'abc-analysis-inventory-prioritize-what-matters' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Inventory Management Insights' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 3: Multi-Location Stock
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Managing Inventory Across Multiple Locations: Best Practices' as "Title",
    'Coordinate stock across branches, warehouses, and retail outlets. Learn transfer workflows, consolidated reporting, and avoiding common multi-location pitfalls.' as "ShortDescription",
    '<h2>Multi-Location Inventory Challenges</h2>
    <p>When stock sits in multiple places, visibility and coordination become critical. Poor multi-location management leads to overstocking in some places and stockouts in others.</p>
    
    <h3>Centralized vs. Decentralized</h3>
    <p>Decide whether each location manages its own stock or a central team allocates. Centralized control suits uniform products; decentralized works when locations have distinct demand patterns.</p>
    
    <h3>Inter-Branch Transfers</h3>
    <p>Document all stock movements between locations. Use transfer orders with approval workflows. Track in-transit inventory so it doesn''t appear available at either location during movement.</p>
    
    <h3>Unified Reporting</h3>
    <p>View total stock, location-wise breakups, and transfer history from one dashboard. This enables better allocation decisions and faster response to demand shifts.</p>
    
    <h3>Conclusion</h3>
    <p>Multi-location inventory works when systems support it. EquiBillBook allows location-wise stock tracking and inter-branch transfers with full audit trails.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Inventory Management Insights' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'multi-location inventory, stock transfer, branch management' as "Taglist",
    'Managing Inventory Across Multiple Locations: Best Practices | EquiBillBook' as "MetaTitle",
    'Coordinate stock across branches, warehouses, and retail outlets. Learn transfer workflows, consolidated reporting, and avoiding common multi-location pitfalls.' as "MetaDescription",
    'multi-location inventory, stock transfer, branch management' as "MetaKeywords",
    'managing-inventory-across-multiple-locations-best-practices' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Inventory Management Insights' AND "CompanyId" = 1 AND "IsDeleted" = false);
