-- =====================================================
-- Blog Posts: Financial Planning & Budgeting Category
-- CompanyId: 1
-- Date: 2025-02-13
-- =====================================================

-- Blog Post 1: SME Budget Framework
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'A Simple Budget Framework for Small Business Owners' as "Title",
    'Create a practical budget without complexity. Revenue projections, expense categories, and variance tracking—all in a format you can actually maintain.' as "ShortDescription",
    '<h2>Budget Basics for SMEs</h2>
    <p>A budget is a plan for your money. It doesn''t need to be complex—just realistic and reviewed regularly.</p>
    
    <h3>Revenue Projection</h3>
    <p>Base projections on historical sales, adjusted for known factors: new customers, price changes, seasonality. Stay conservative—overestimating revenue leads to overspending.</p>
    
    <h3>Expense Categories</h3>
    <ul>
        <li>Fixed: rent, salaries, subscriptions—predictable monthly</li>
        <li>Variable: materials, commissions—tied to sales volume</li>
        <li>One-time: equipment, renovations—plan and save in advance</li>
    </ul>
    
    <h3>Variance Tracking</h3>
    <p>Compare actual revenue and expenses to budget each month. Investigate large variances. Adjust either the budget (if assumptions changed) or operations (if spending is off track).</p>
    
    <h3>Tools</h3>
    <p>Spreadsheets work for simple budgets. Accounting software with budget vs. actual reports automates the comparison and saves time.</p>
    
    <h3>Conclusion</h3>
    <p>A simple budget gives direction. EquiBillBook helps track actuals so you can compare against your plan and stay on course.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Financial Planning & Budgeting' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'SME budget, financial planning, small business budgeting' as "Taglist",
    'A Simple Budget Framework for Small Business Owners | EquiBillBook' as "MetaTitle",
    'Create a practical budget without complexity. Revenue projections, expense categories, and variance tracking—all in a format you can actually maintain.' as "MetaDescription",
    'SME budget, financial planning, small business budgeting' as "MetaKeywords",
    'simple-budget-framework-small-business-owners' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Financial Planning & Budgeting' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 2: Cash Flow Forecasting
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Cash Flow Forecasting: Predict Shortfalls Before They Hurt' as "Title",
    'Anticipate cash gaps before they become crises. Learn to build a simple 90-day cash flow forecast using your sales and payment data.' as "ShortDescription",
    '<h2>Why Forecast Cash Flow?</h2>
    <p>Profit on paper doesn''t mean cash in the bank. Sales on credit, upfront expenses, and seasonal patterns create timing mismatches. Forecasting helps you see gaps before they hit.</p>
    
    <h3>Building a Simple Forecast</h3>
    <p>Project cash inflows: when do you expect to get paid? Use historical collection patterns. Project outflows: when are bills, salaries, and loan payments due? Match them week by week or month by month.</p>
    
    <h3>Key Inputs</h3>
    <ul>
        <li>Outstanding receivables and expected collection dates</li>
        <li>Upcoming payables and their due dates</li>
        <li>Planned purchases or investments</li>
        <li>Tax payment deadlines</li>
    </ul>
    
    <h3>When Shortfalls Appear</h3>
    <p>If forecast shows negative cash, act early: accelerate collections, delay non-critical payments, arrange a short-term loan, or cut discretionary spending. Options shrink when you''re in the middle of a crisis.</p>
    
    <h3>Conclusion</h3>
    <p>Cash flow forecasting is a survival skill. Use your accounting data to build and update forecasts. EquiBillBook''s receivables and payables reports provide the raw data you need.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Financial Planning & Budgeting' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'cash flow forecast, cash flow planning, SME finance' as "Taglist",
    'Cash Flow Forecasting: Predict Shortfalls Before They Hurt | EquiBillBook' as "MetaTitle",
    'Anticipate cash gaps before they become crises. Learn to build a simple 90-day cash flow forecast using your sales and payment data.' as "MetaDescription",
    'cash flow forecast, cash flow planning, SME finance' as "MetaKeywords",
    'cash-flow-forecasting-predict-shortfalls-before-they-hurt' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Financial Planning & Budgeting' AND "CompanyId" = 1 AND "IsDeleted" = false);

-- Blog Post 3: Emergency Fund
INSERT INTO public."tblBlog" (
    "Title", "ShortDescription", "Description", "Image", "BlogCategoryId", 
    "Taglist", "MetaTitle", "MetaDescription", "MetaKeywords", "UniqueSlug", 
    "PublishedDate", "ViewCount", "CompanyId", "IsActive", "IsDeleted", 
    "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn"
)
SELECT 
    'Building a Business Emergency Fund: Why and How' as "Title",
    'Businesses face unexpected expenses and revenue dips. An emergency fund provides cushion. Learn how much to save and where to keep it.' as "ShortDescription",
    '<h2>The Case for Reserves</h2>
    <p>Equipment breaks. Key customers delay payment. A slow season hits. Without reserves, one shock can derail operations or force expensive emergency borrowing.</p>
    
    <h3>How Much to Save</h3>
    <p>A common target: 3-6 months of essential operating expenses (rent, salaries, critical supplier payments). Adjust based on your industry''s volatility and your risk tolerance.</p>
    
    <h3>Where to Keep It</h3>
    <p>Liquid and low-risk. A separate savings account or short-term fixed deposit. Not in business operations—temptation to dip in for "opportunities" is high. Define clear rules for when to use it.</p>
    
    <h3>Building the Fund</h3>
    <p>Start small. Allocate a percentage of revenue or profit each month. Treat it like a non-negotiable expense. Increase the allocation when business is good so you build faster.</p>
    
    <h3>When to Use It</h3>
    <p>True emergencies: revenue shortfall threatening payroll, critical equipment failure, unexpected compliance cost. Not for expansion, marketing experiments, or optional upgrades.</p>
    
    <h3>Conclusion</h3>
    <p>Emergency funds reduce stress and prevent fire sales. Track your savings progress in your financial reports. EquiBillBook helps you monitor cash position and plan for reserves.</p>' as "Description",
    '' as "Image",
    (SELECT "BlogCategoryId" FROM public."tblBlogCategory" WHERE "CategoryName" = 'Financial Planning & Budgeting' AND "CompanyId" = 1 AND "IsDeleted" = false LIMIT 1) as "BlogCategoryId",
    'emergency fund, business reserves, SME finance' as "Taglist",
    'Building a Business Emergency Fund: Why and How | EquiBillBook' as "MetaTitle",
    'Businesses face unexpected expenses and revenue dips. An emergency fund provides cushion. Learn how much to save and where to keep it.' as "MetaDescription",
    'emergency fund, business reserves, SME finance' as "MetaKeywords",
    'building-business-emergency-fund-why-and-how' as "UniqueSlug",
    CURRENT_TIMESTAMP as "PublishedDate",
    0 as "ViewCount",
    1 as "CompanyId",
    true as "IsActive",
    false as "IsDeleted",
    1 as "AddedBy",
    CURRENT_TIMESTAMP as "AddedOn",
    1 as "ModifiedBy",
    NULL as "ModifiedOn"
WHERE EXISTS (SELECT 1 FROM public."tblBlogCategory" WHERE "CategoryName" = 'Financial Planning & Budgeting' AND "CompanyId" = 1 AND "IsDeleted" = false);
