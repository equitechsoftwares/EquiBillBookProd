# Blog Categories Insert Script

This script inserts SEO-optimized blog categories for EquiBillBook to improve organic keyword ranking and traffic.

## Database Information
- **Database**: PostgreSQL
- **Table**: `public.tblBlogCategory`
- **CompanyId**: 1
- **Schema**: public

## Prerequisites

Before running this script:
1. Ensure you have access to the PostgreSQL database
2. Update `AddedBy` and `ModifiedBy` values if your admin user ID is not 1
3. Verify that CompanyId = 1 exists in your system

## SQL Script

```sql
-- =====================================================
-- Blog Categories Insert Script
-- CompanyId: 1
-- Date: 2025-02-12
-- =====================================================
-- Note: Update AddedBy and ModifiedBy with actual UserId if needed
-- =====================================================

-- High Priority Categories (High Search Volume)
INSERT INTO public."tblBlogCategory" ("CategoryName", "IsActive", "IsDeleted", "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn", "CompanyId")
VALUES 
('GST Compliance & Updates', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Business Growth & Strategy', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Accounting & Finance Tips', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Inventory Management Insights', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Tax Planning & Savings', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1);

-- Medium Priority Categories (Good Long-Tail Potential)
INSERT INTO public."tblBlogCategory" ("CategoryName", "IsActive", "IsDeleted", "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn", "CompanyId")
VALUES 
('Industry Trends & News', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Success Stories & Case Studies', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Business Automation & Efficiency', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Digital Transformation', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Financial Planning & Budgeting', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1);

-- Niche Categories (Low Competition, High Intent)
INSERT INTO public."tblBlogCategory" ("CategoryName", "IsActive", "IsDeleted", "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn", "CompanyId")
VALUES 
('E-Invoicing & Digital Payments', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Multi-Location Business Insights', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Retail Business Tips', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Restaurant & Hospitality', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Service Business Management', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1);

-- Content Marketing Categories
INSERT INTO public."tblBlogCategory" ("CategoryName", "IsActive", "IsDeleted", "AddedBy", "AddedOn", "ModifiedBy", "ModifiedOn", "CompanyId")
VALUES 
('Software Comparisons', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Business Challenges & Solutions', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1),
('Regulatory Updates', true, false, 1, CURRENT_TIMESTAMP, 1, NULL, 1);

-- Verify the insertions
SELECT "BlogCategoryId", "CategoryName", "IsActive", "CompanyId" 
FROM public."tblBlogCategory" 
WHERE "CompanyId" = 1 AND "IsDeleted" = false 
ORDER BY "BlogCategoryId" DESC;
```

## How to Execute

### Option 1: Using pgAdmin
1. Open pgAdmin
2. Connect to your database
3. Right-click on the database → **Query Tool**
4. Paste the SQL script
5. Click **Execute** (F5)

### Option 2: Using psql Command Line
```bash
psql -h localhost -p 5433 -U postgres -d EquiBillBook -f InsertBlogCategories.sql
```

### Option 3: Using Database Management Tool
1. Open your preferred database management tool (DBeaver, DataGrip, etc.)
2. Connect to the PostgreSQL database
3. Open a new SQL query window
4. Paste and execute the script

## Categories Overview

### High Priority Categories (5 categories)
These categories target high-volume keywords and should be prioritized for content creation:

1. **GST Compliance & Updates** - Targets keywords like "GST compliance tips", "GST updates 2025"
2. **Business Growth & Strategy** - Targets "small business growth tips", "SME business advice"
3. **Accounting & Finance Tips** - Targets "accounting tips for small business", "financial management tips"
4. **Inventory Management Insights** - Targets "inventory management tips", "stock optimization"
5. **Tax Planning & Savings** - Targets "tax saving tips for business", "GST input credit"

### Medium Priority Categories (5 categories)
Good for long-tail keyword targeting:

6. **Industry Trends & News** - "accounting software trends", "business technology trends"
7. **Success Stories & Case Studies** - "business success stories", "SME case studies"
8. **Business Automation & Efficiency** - "business automation tips", "improve business efficiency"
9. **Digital Transformation** - "digital transformation for small business", "cloud accounting benefits"
10. **Financial Planning & Budgeting** - "business budgeting tips", "financial planning for SMEs"

### Niche Categories (5 categories)
Low competition, high-intent keywords:

11. **E-Invoicing & Digital Payments** - "e-invoicing benefits", "digital payment solutions"
12. **Multi-Location Business Insights** - "multi-location business management", "chain business tips"
13. **Retail Business Tips** - "retail business management", "retail store tips"
14. **Restaurant & Hospitality** - "restaurant management tips", "hospitality business advice"
15. **Service Business Management** - "service business tips", "freelancer business advice"

### Content Marketing Categories (3 categories)
For comparison and problem-solving content:

16. **Software Comparisons** - "best GST billing software", "accounting software comparison"
17. **Business Challenges & Solutions** - "common business problems", "business challenges solutions"
18. **Regulatory Updates** - "GST rule changes", "business compliance updates"

## Verification

After running the script, verify the insertions:

```sql
SELECT 
    "BlogCategoryId", 
    "CategoryName", 
    "IsActive", 
    "CompanyId",
    "AddedOn"
FROM public."tblBlogCategory" 
WHERE "CompanyId" = 1 
    AND "IsDeleted" = false 
ORDER BY "BlogCategoryId" DESC;
```

Expected result: **18 categories** should be inserted.

## Notes

- All categories are set to `IsActive = true` and `IsDeleted = false`
- `AddedBy` and `ModifiedBy` are set to `1` (update if your admin user ID is different)
- `ModifiedOn` is set to `NULL` (will be updated when categories are modified)
- `AddedOn` uses `CURRENT_TIMESTAMP` (current date/time)

## Troubleshooting

### Error: Duplicate Category Name
If you get a duplicate category name error, check existing categories:
```sql
SELECT "CategoryName" FROM public."tblBlogCategory" 
WHERE "CompanyId" = 1 AND "IsDeleted" = false;
```

### Error: Foreign Key Constraint
If you get a foreign key error for `AddedBy` or `ModifiedBy`, verify the user exists:
```sql
SELECT "UserId" FROM public."tblUser" WHERE "UserId" = 1;
```

### Error: CompanyId Not Found
Verify CompanyId exists:
```sql
SELECT "CompanyId" FROM public."tblUser" WHERE "CompanyId" = 1 LIMIT 1;
```

## Next Steps

After successfully inserting the categories:

1. **Create Blog Posts**: Start creating blog posts for each category
2. **SEO Optimization**: Use relevant keywords in blog titles and content
3. **Content Calendar**: Plan 3-5 blog posts per category per month
4. **Monitor Performance**: Track which categories drive the most traffic
5. **Update Regularly**: Keep content fresh and up-to-date

## Support

For issues or questions, refer to:
- Blog Category Controller: `Controllers/WebApi/BlogCategoryController.cs`
- Blog Category Model: `Models/ClsBlogCategory.cs`
- Admin Interface: `/AdminBlogCategory/Index`
