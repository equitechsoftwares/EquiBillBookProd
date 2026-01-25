# Tiered Enterprise Manufacturing Module - Implementation Plan

## Overview

This document outlines the implementation plan for a comprehensive tiered manufacturing module suitable for small, mid-size, and large manufacturing units. The module uses a feature flag system to control which features are available, allowing businesses to use only what they need.

**Target Audience:**
- Small Manufacturing Units (1-10 employees)
- Mid-Size Manufacturing Units (10-50 employees)
- Large/Enterprise Manufacturing Units (50+ employees)

---

## Tier Structure

### Tier 1: Basic Manufacturing (Small Units)

**Target:** Small manufacturing units with simple production processes

**Features Included:**
- ✅ Single-level BOM (no sub-assemblies)
- ✅ Simple work orders
- ✅ Basic production recording
- ✅ Material consumption tracking
- ✅ Finished goods production
- ✅ Basic costing (material + simple labor rate)
- ✅ Basic reports (Production, BOM, Material Consumption)
- ✅ Lot/batch tracking (uses existing system)

**Settings Required:**
```
EnableManufacturing = true
(All other advanced features = false)
```

**Use Cases:**
- Small workshops
- Simple assembly operations
- Basic product manufacturing
- Single location operations

---

### Tier 2: Standard Manufacturing (Mid-Size Units)

**Target:** Mid-size manufacturing units with moderate complexity

**Features Included:**
- ✅ All Tier 1 features
- ✅ Multi-level BOM (sub-assemblies)
- ✅ Work centers (basic)
- ✅ Production routing
- ✅ Material requisition
- ✅ Basic quality control
- ✅ Labor tracking (basic)
- ✅ Co-products/by-products
- ✅ Scrap tracking
- ✅ Production templates

**Settings Required:**
```
EnableManufacturing = true
EnableMultiLevelBOM = true
EnableWorkCenters = true
EnableMaterialRequisition = true
EnableBasicQC = true
EnableLaborTracking = true
EnableCoProducts = true
EnableScrapTracking = true
EnableProductionTemplates = true
EnableProductionRouting = true
```

**Use Cases:**
- Medium-sized factories
- Multi-stage production
- Quality control requirements
- Multiple work centers
- Complex product structures

---

### Tier 3: Enterprise Manufacturing (Large Units)

**Target:** Large manufacturing units with complex production processes

**Features Included:**
- ✅ All Tier 1 & Tier 2 features
- ✅ Advanced production scheduling (Gantt charts)
- ✅ Full MRP (Material Requirements Planning)
- ✅ Advanced quality control workflows
- ✅ Detailed variance analysis
- ✅ Machine maintenance integration
- ✅ Advanced WIP tracking
- ✅ Production backflushing
- ✅ Material substitution
- ✅ Alternative BOMs
- ✅ Advanced labor tracking
- ✅ Capacity planning

**Settings Required:**
```
EnableManufacturing = true
(All feature flags = true - full enterprise mode)
```

**Use Cases:**
- Large manufacturing facilities
- Complex supply chains
- Advanced planning requirements
- Multi-plant operations
- Enterprise-level reporting

---

## Database Models

### Core Manufacturing Models

#### 1. Bill of Materials (BOM)
- `ClsBillOfMaterial` - BOM Master
- `ClsBillOfMaterialDetails` - BOM Components
- `ClsAlternativeBOM` - Alternative BOM versions (Tier 3)
- `ClsBOMSubstitution` - Material substitution rules (Tier 3)

#### 2. Work Orders
- `ClsWorkOrder` - Work Order Master
- `ClsWorkOrderDetails` - Work Order Items
- `ClsWorkOrderMaterial` - Material Requirements
- `ClsWorkOrderRouting` - Work order routing steps (Tier 2+)

#### 3. Production Process
- `ClsProduction` - Production Master
- `ClsProductionDetails` - Produced Items
- `ClsProductionMaterial` - Material Consumption
- `ClsProductionCoProduct` - Co-products/by-products (Tier 2+)
- `ClsProductionScrap` - Scrap and waste tracking (Tier 2+)

#### 4. Production Routing (Tier 2+)
- `ClsProductionRouting` - Routing Master
- `ClsProductionRoutingStep` - Routing Steps

### Enterprise Models (Tier 2 & 3)

#### 5. Work Centers & Machines (Tier 2+)
- `ClsWorkCenter` - Work Center Master
- `ClsMachine` - Machine/Equipment Master
- `ClsMachineCapacity` - Machine capacity tracking
- `ClsWorkCenterCost` - Work center cost rates

#### 6. Material Requisition (Tier 2+)
- `ClsMaterialRequisition` - Material Requisition Master
- `ClsMaterialRequisitionDetails` - Material Requisition Items

#### 7. Quality Control (Tier 2+)
- `ClsQualityControl` - QC Master
- `ClsQualityControlCheckpoint` - QC Checkpoints
- `ClsQualityControlResult` - QC Results
- `ClsQualityRejection` - Rejection tracking

#### 8. Labor Tracking (Tier 2+)
- `ClsLaborTracking` - Labor Time Tracking
- `ClsLaborOperation` - Labor Operations

#### 9. Work in Progress (WIP) Tracking (Tier 3)
- `ClsWIPTracking` - WIP Tracking
- `ClsWIPStage` - WIP Stages

#### 10. Production Scheduling (Tier 3)
- `ClsProductionSchedule` - Production Schedule
- `ClsScheduleConflict` - Schedule conflict tracking

#### 11. Material Requirements Planning (MRP) (Tier 3)
- `ClsMRPPlan` - MRP Plan
- `ClsMRPSuggestion` - MRP Suggestions

#### 12. Variance Analysis (Tier 3)
- `ClsProductionVariance` - Production Variance

#### 13. Production Templates (Tier 2+)
- `ClsProductionTemplate` - Production Template

#### 14. Machine Maintenance Integration (Tier 3)
- `ClsMachineMaintenance` - Machine Maintenance Link

#### 15. Manufacturing Settings
- `ClsManufacturingSettings` - Settings with tiered feature flags

---

## Manufacturing Settings Model

### Feature Flags

#### Master Control
- `EnableManufacturing` (bool) - Master switch for entire module

#### Tier 2 (Standard) Features
- `EnableMultiLevelBOM` (bool) - Enable sub-assemblies in BOM
- `EnableWorkCenters` (bool) - Enable work center management
- `EnableMaterialRequisition` (bool) - Enable material requisition process
- `EnableBasicQC` (bool) - Enable basic quality control
- `EnableLaborTracking` (bool) - Enable basic labor time tracking
- `EnableCoProducts` (bool) - Enable co-products/by-products
- `EnableScrapTracking` (bool) - Enable scrap/waste tracking
- `EnableProductionTemplates` (bool) - Enable production templates
- `EnableProductionRouting` (bool) - Enable production routing

#### Tier 3 (Enterprise) Features
- `EnableProductionScheduling` (bool) - Enable advanced scheduling with Gantt
- `EnableMRP` (bool) - Enable Material Requirements Planning
- `EnableAdvancedQC` (bool) - Enable advanced QC workflows
- `EnableVarianceAnalysis` (bool) - Enable variance analysis
- `EnableMachineMaintenance` (bool) - Enable machine maintenance integration
- `EnableWIPTracking` (bool) - Enable advanced WIP tracking
- `EnableBackflushing` (bool) - Enable production backflushing
- `EnableMaterialSubstitution` (bool) - Enable material substitution
- `EnableAlternativeBOM` (bool) - Enable alternative BOM versions
- `EnableAdvancedLaborTracking` (bool) - Enable detailed labor tracking
- `EnableCapacityPlanning` (bool) - Enable capacity planning

### Configuration Settings
- `DefaultMaterialAccountId` (long) - Default account for material costs
- `DefaultLaborAccountId` (long) - Default account for labor costs
- `DefaultOverheadAccountId` (long) - Default account for overhead costs
- `DefaultWIPAccountId` (long) - Default account for work-in-progress
- `DefaultFinishedGoodsAccountId` (long) - Default account for finished goods
- `CostingMethod` (string) - "Standard", "Average", "FIFO"
- `DefaultLaborRate` (decimal) - Default labor rate per hour
- `DefaultOverheadRate` (decimal) - Default overhead rate percentage
- `EnableBackflushingByDefault` (bool) - Default backflushing setting
- `BackflushingMethod` (string) - "Completion", "Operation"
- `QCRequiredForCompletion` (bool) - Require QC before production completion
- `MRPLeadTimeDays` (int) - Default lead time for MRP
- `MRPSafetyStockPercent` (decimal) - Safety stock percentage for MRP

---

## Feature Dependencies

Some features depend on others being enabled:

| Feature | Requires |
|---------|----------|
| `EnableProductionScheduling` | `EnableWorkCenters = true` |
| `EnableMRP` | `EnableMultiLevelBOM = true` |
| `EnableBackflushing` | `EnableWorkCenters = true` |
| `EnableVarianceAnalysis` | `EnableLaborTracking = true` |
| `EnableAdvancedQC` | `EnableBasicQC = true` |
| `EnableAdvancedLaborTracking` | `EnableLaborTracking = true` |

**Note:** Validation should enforce these dependencies in the settings UI.

---

## Controllers

### MVC Controllers (`Controllers/Customer/Manufacturing/`)
- `BillOfMaterialController.cs` - BOM management
- `WorkOrderController.cs` - Work order management
- `ProductionController.cs` - Production process management
- `WorkCenterController.cs` - Work center and machine management (Tier 2+)
- `MaterialRequisitionController.cs` - Material requisition management (Tier 2+)
- `QualityControlController.cs` - Quality control management (Tier 2+)
- `ProductionSchedulingController.cs` - Production scheduling with Gantt view (Tier 3)
- `MRPController.cs` - Material Requirements Planning (Tier 3)
- `ManufacturingReportsController.cs` - Comprehensive manufacturing reports
- `VarianceAnalysisController.cs` - Variance analysis and reporting (Tier 3)
- `ManufacturingSettingsController.cs` - Manufacturing settings management

### API Controllers (`Controllers/WebApi/`)
- `BillOfMaterialController.cs` - BOM CRUD operations
- `WorkOrderController.cs` - Work order CRUD operations
- `ProductionController.cs` - Production CRUD operations
- `WorkCenterController.cs` - Work center and machine CRUD (Tier 2+)
- `MaterialRequisitionController.cs` - Material requisition CRUD (Tier 2+)
- `QualityControlController.cs` - QC operations (Tier 2+)
- `LaborTrackingController.cs` - Labor time tracking (Tier 2+)
- `WIPTrackingController.cs` - WIP tracking (Tier 3)
- `ProductionSchedulingController.cs` - Scheduling algorithms, Gantt data (Tier 3)
- `MRPController.cs` - MRP calculations and suggestions (Tier 3)
- `VarianceAnalysisController.cs` - Variance calculations and reports (Tier 3)
- `ProductionTemplateController.cs` - Template management (Tier 2+)
- `ManufacturingSettingsController.cs` - Settings management

---

## Views

### Bill of Materials (`Views/Manufacturing/BillOfMaterial/`)
- `Index.cshtml` - BOM list with search/filter
- `Add.cshtml` - Create new BOM
- `Edit.cshtml` - Edit existing BOM
- `View.cshtml` - View BOM details
- `AlternativeBOM.cshtml` - Manage alternative BOMs (Tier 3)
- `Components/_Modals.cshtml` - Item selection modals

### Work Orders (`Views/Manufacturing/WorkOrder/`)
- `Index.cshtml` - Work order list
- `Add.cshtml` - Create work order
- `Edit.cshtml` - Edit work order
- `View.cshtml` - View work order details
- `Components/_Modals.cshtml` - BOM/item selection modals

### Production (`Views/Manufacturing/Production/`)
- `Index.cshtml` - Production list
- `Add.cshtml` - Record production
- `Edit.cshtml` - Edit production
- `View.cshtml` - View production details
- `Backflushing.cshtml` - Backflushing interface (Tier 3)
- `Components/_Modals.cshtml` - Material substitution modal, scrap entry modal

### Work Centers (`Views/Manufacturing/WorkCenter/`) - Tier 2+
- `Index.cshtml` - Work center list
- `Add.cshtml` - Create work center
- `Edit.cshtml` - Edit work center
- `MachineManagement.cshtml` - Machine management
- `CapacityPlanning.cshtml` - Capacity planning view (Tier 3)

### Production Scheduling (`Views/Manufacturing/ProductionScheduling/`) - Tier 3
- `Index.cshtml` - Scheduling dashboard
- `GanttView.cshtml` - Gantt chart visualization
- `CapacityView.cshtml` - Capacity view
- `DragDropScheduling.cshtml` - Drag-drop scheduling interface

### Material Requisition (`Views/Manufacturing/MaterialRequisition/`) - Tier 2+
- `Index.cshtml` - Material requisition list
- `Add.cshtml` - Create material requisition
- `Edit.cshtml` - Edit material requisition
- `Issue.cshtml` - Material issue interface

### Quality Control (`Views/Manufacturing/QualityControl/`) - Tier 2+
- `Index.cshtml` - QC list
- `Inspection.cshtml` - QC inspection interface
- `CheckpointSetup.cshtml` - QC checkpoint configuration
- `RejectionHandling.cshtml` - Rejection management

### MRP (`Views/Manufacturing/MRP/`) - Tier 3
- `Index.cshtml` - MRP planning dashboard
- `MaterialRequirements.cshtml` - Material requirements view
- `Suggestions.cshtml` - MRP suggestions

### Manufacturing Settings (`Views/ManufacturingSettings/`)
- `Index.cshtml` - Manufacturing settings page with tiered feature toggles

### Reports (`Views/ManufacturingReports/`)
- `ProductionReport.cshtml` - Production summary report
- `BOMReport.cshtml` - BOM listing and cost report
- `MaterialConsumptionReport.cshtml` - Raw material consumption report
- `WorkOrderStatusReport.cshtml` - Work order status report
- `ManufacturingCostReport.cshtml` - Cost analysis report
- `VarianceAnalysisReport.cshtml` - Variance analysis report (Tier 3)
- `MRPReport.cshtml` - MRP report (Tier 3)
- `QualityControlReport.cshtml` - QC report (Tier 2+)
- `LaborEfficiencyReport.cshtml` - Labor efficiency report (Tier 2+)
- `CapacityUtilizationReport.cshtml` - Capacity utilization report (Tier 3)

---

## JavaScript Files

### `Content/JQuery/Customer/Manufacturing/`
- `BillOfMaterial.js` - BOM CRUD operations, cost calculation
- `WorkOrder.js` - Work order management, BOM integration
- `Production.js` - Production recording, stock updates
- `WorkCenter.js` - Work center and machine management (Tier 2+)
- `Scheduling.js` - Production scheduling, Gantt chart integration (Tier 3)
- `MRP.js` - MRP calculations, material requirements display (Tier 3)
- `QualityControl.js` - QC workflows, inspection handling (Tier 2+)
- `LaborTracking.js` - Labor time tracking (Tier 2+)
- `WIPTracking.js` - WIP tracking at each stage (Tier 3)
- `VarianceAnalysis.js` - Variance calculations and reporting (Tier 3)
- `ManufacturingReports.js` - Report generation and export
- `ManufacturingSettings.js` - Settings management with feature flag handling

---

## Feature Gating Implementation

### Pattern to Follow

All manufacturing features must check `ManufacturingSettings` before:

1. **API Controllers:** Validate feature flags before processing requests
2. **MVC Controllers:** Check flags in actions, redirect if feature disabled
3. **Views:** Use conditional rendering (`@if (ViewBag.ManufacturingSettings.EnableFeature)`)
4. **JavaScript:** Check settings before enabling features, hide/show UI elements
5. **Menu/Navigation:** Show/hide menu items based on enabled features

### Example Code

```csharp
// In Controller
var settings = GetManufacturingSettings(companyId);
if (!settings.EnableManufacturing)
{
    return RedirectToAction("Index", "Dashboard");
}

if (settings.EnableMultiLevelBOM)
{
    // Enable multi-level BOM functionality
}
```

```razor
@* In View *@
@if (ViewBag.ManufacturingSettings.EnableMultiLevelBOM)
{
    <div class="multi-level-bom-section">
        <!-- Multi-level BOM UI -->
    </div>
}
```

```javascript
// In JavaScript
if (manufacturingSettings.EnableMultiLevelBOM) {
    // Enable multi-level BOM functionality
    initializeMultiLevelBOM();
}
```

---

## Key Features by Tier

### Tier 1: Basic Features
1. **Single-Level BOM**
   - Create BOM for finished goods
   - Define components/raw materials with quantities
   - Calculate BOM cost (material + simple labor)
   - BOM validation

2. **Simple Work Orders**
   - Create work orders from BOM
   - Work order statuses: Draft, In Progress, Completed, Cancelled
   - Material requirement calculation
   - Stock availability checking

3. **Basic Production**
   - Record production completion
   - Consume raw materials (reduce stock)
   - Produce finished goods (increase stock)
   - Track lot numbers and batch numbers
   - Support partial production

4. **Basic Costing**
   - Material cost (from purchase/opening stock)
   - Simple labor cost (configurable rate)
   - Total production cost calculation
   - Cost per unit calculation

5. **Basic Reports**
   - Production summary report
   - BOM cost report
   - Material consumption report
   - Work order status report

### Tier 2: Standard Features
1. **Multi-Level BOM**
   - Hierarchical BOM structure (sub-assemblies)
   - Multi-level cost rollup calculation
   - BOM versioning

2. **Work Centers**
   - Work center master data
   - Machine/equipment management
   - Work center cost rates
   - Machine assignment to work orders

3. **Production Routing**
   - Routing steps definition
   - Work center assignment to steps
   - Time estimates

4. **Material Requisition**
   - Separate material requisition process
   - Material issue tracking
   - Stock reservation

5. **Basic Quality Control**
   - QC checkpoint configuration
   - Inspection workflows
   - QC results recording
   - Rejection handling

6. **Labor Tracking**
   - Labor time tracking per operation
   - Employee assignment
   - Labor cost calculation

7. **Co-products & By-products**
   - Multiple outputs from single production
   - Cost allocation
   - Stock tracking

8. **Scrap Tracking**
   - Scrap quantity tracking
   - Scrap reasons
   - Scrap cost calculation

### Tier 3: Enterprise Features
1. **Advanced Production Scheduling**
   - Production scheduling algorithms
   - Gantt chart visualization
   - Drag-drop scheduling interface
   - Schedule conflict detection
   - Capacity planning

2. **Material Requirements Planning (MRP)**
   - Automatic material planning based on demand
   - Sales order integration
   - Suggested purchase orders
   - Suggested work orders for sub-assemblies
   - Lead time consideration

3. **Advanced Quality Control**
   - Advanced QC workflows
   - Quality standards and tolerances
   - Quality certificates
   - Quality metrics

4. **Variance Analysis**
   - Compare planned vs actual materials
   - Compare planned vs actual time
   - Compare planned vs actual cost
   - Variance reporting

5. **Advanced WIP Tracking**
   - WIP tracking at each production stage
   - WIP location tracking
   - WIP valuation
   - WIP aging analysis

6. **Production Backflushing**
   - Automatic material consumption based on output
   - Backflushing configuration per item/BOM
   - Backflushing variance handling

7. **Material Substitution**
   - Material substitution rules in BOM
   - Substitution during production
   - Substitution tracking

8. **Alternative BOMs**
   - Different BOM versions for same product
   - BOM selection based on criteria

9. **Machine Maintenance Integration**
   - Link production to machine maintenance
   - Maintenance schedule tracking
   - Machine downtime tracking

10. **Capacity Planning**
    - Capacity utilization tracking
    - Load balancing
    - Capacity reports

---

## Integration Points

### Existing Systems
- **Items:** Link BOM to items, work orders to items
- **Stock Management:** Consume/produce stock automatically, multi-warehouse support
- **Purchase:** Use purchase prices for material costing, MRP suggestions
- **Sales:** Track which sales orders require manufacturing, MRP demand
- **Accounts:** Manufacturing cost journal entries (if accounting integration enabled)
- **Branches:** Multi-branch manufacturing support
- **Users:** Labor tracking, QC inspector assignment

---

## Implementation Checklist

### Phase 1: Core Models & Settings
- [ ] Create core manufacturing models (BOM, WorkOrder, Production)
- [ ] Create ManufacturingSettings model with all feature flags
- [ ] Add DbSets to ConnectionContext.cs
- [ ] Create database migration scripts

### Phase 2: Basic Manufacturing (Tier 1)
- [ ] Create BOM API and MVC controllers
- [ ] Create Work Order API and MVC controllers
- [ ] Create Production API and MVC controllers
- [ ] Create basic views (BOM, Work Order, Production)
- [ ] Implement stock integration
- [ ] Create basic reports
- [ ] Create Manufacturing Settings UI

### Phase 3: Standard Features (Tier 2)
- [ ] Create Work Center models and controllers
- [ ] Create Material Requisition models and controllers
- [ ] Create Quality Control models and controllers
- [ ] Create Labor Tracking models and controllers
- [ ] Implement multi-level BOM support
- [ ] Create standard feature views
- [ ] Update settings UI for Tier 2 features

### Phase 4: Enterprise Features (Tier 3)
- [ ] Create Production Scheduling models and controllers
- [ ] Create MRP models and controllers
- [ ] Create Variance Analysis models and controllers
- [ ] Create WIP Tracking models and controllers
- [ ] Implement Gantt chart visualization
- [ ] Create enterprise feature views
- [ ] Update settings UI for Tier 3 features

### Phase 5: Feature Gating & Testing
- [ ] Implement feature gating in all controllers
- [ ] Implement conditional rendering in all views
- [ ] Update menu/navigation based on features
- [ ] Test Tier 1 (Basic) functionality
- [ ] Test Tier 2 (Standard) functionality
- [ ] Test Tier 3 (Enterprise) functionality
- [ ] Test feature flag dependencies
- [ ] Performance testing and optimization

---

## File Estimates

### New Files
- **Models:** 25-30 files
- **Controllers:** 20-25 files
- **Views:** 50-60 files
- **JavaScript:** 10-12 files
- **CSS:** Updates to existing files

### Files to Modify
- `ConnectionContext.cs` - Add DbSets
- Menu configuration - Add manufacturing menu items
- Navigation - Add manufacturing links
- Existing stock management - Enhance for manufacturing

---

## Notes

- Follow existing code patterns from Purchase/Sales modules
- Use same UI components and styling
- Maintain consistency with existing stock management
- Support multi-branch operations
- Ensure proper error handling and validation
- Add proper logging for manufacturing operations
- Optimize database queries for performance
- **IMPORTANT:** Always check feature flags before rendering UI or processing requests
- **IMPORTANT:** Hide disabled features from menus and navigation
- **IMPORTANT:** Provide clear error messages when disabled features are accessed
- **IMPORTANT:** Settings should be company-specific (stored per CompanyId)

---

## Success Criteria

### Tier 1 (Basic)
- ✅ Small manufacturing units can create BOMs, work orders, and record production
- ✅ Stock is automatically updated (consumed/produced)
- ✅ Basic costing and reports are available
- ✅ All features work without any Tier 2/3 flags enabled

### Tier 2 (Standard)
- ✅ Mid-size units can use multi-level BOMs, work centers, and QC
- ✅ Material requisition process works correctly
- ✅ Co-products and scrap tracking function properly
- ✅ All Tier 2 features integrate seamlessly with Tier 1

### Tier 3 (Enterprise)
- ✅ Large units can use advanced scheduling, MRP, and variance analysis
- ✅ Gantt charts display correctly
- ✅ MRP calculations are accurate
- ✅ All enterprise features work together harmoniously

---

**Document Version:** 1.0  
**Last Updated:** 2024  
**Status:** Planning Phase

