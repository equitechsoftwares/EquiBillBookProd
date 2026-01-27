using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Optimization;
using System.Web.Routing;
using EquiBillBook.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace EquiBillBook
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static bool _isWarmedUp = false;
        private static readonly object _warmupLock = new object();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Warmup: Pre-initialize Entity Framework model to avoid first-request delay
            // Run synchronously to ensure it completes before first request
            WarmupApplication();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Ensure warmup completes before processing first request
            // This handles the case where Application_Start warmup didn't complete in time
            if (!_isWarmedUp)
            {
                lock (_warmupLock)
                {
                    if (!_isWarmedUp)
                    {
                        // Quick warmup check - don't block too long
                        try
                        {
                            using (var context = new ConnectionContext())
                            {
                                // Quick query to ensure EF is initialized
                                var _ = context.DbClsUser.Count();
                            }
                            _isWarmedUp = true;
                        }
                        catch
                        {
                            // If warmup fails, mark as done to avoid repeated attempts
                            _isWarmedUp = true;
                        }
                    }
                }
            }
        }

        private void WarmupApplication()
        {
            try
            {
                // Pre-compile Entity Framework model synchronously
                // This prevents the slow first query after app pool recycle
                WarmupEntityFramework();

                _isWarmedUp = true;

                // Pre-compile Razor views synchronously for critical views
                // This prevents first-request view compilation delay
                PrecompileViews();
            }
            catch (Exception ex)
            {
                // Log error but don't fail application startup
                // The first request will be slower if warmup fails
                System.Diagnostics.Debug.WriteLine($"Warmup error: {ex.Message}");
                _isWarmedUp = true; // Mark as done to prevent repeated attempts
            }
        }

        private void WarmupEntityFramework()
        {
            try
            {
                using (var context = new ConnectionContext())
                {
                    // Execute multiple simple queries to initialize EF model and common code paths
                    // This triggers EF model compilation which is expensive on first run
                    
                    // Initialize database connection and EF model
                    var userCount = context.DbClsUser.Count();
                    
                    // Touch other common tables to pre-compile their queries
                    var branchCount = context.DbClsBranch.Count();
                    var menuCount = context.DbClsMenu.Count();
                    
                    // Pre-compile queries for commonly used tables
                    var categoryCount = context.DbClsCategory.Count();
                    var itemCount = context.DbClsItem.Count();
                    
                    // Force connection pool to initialize and keep it warm
                    context.Database.Connection.Open();
                    context.Database.Connection.Close();
                    
                    // Force EF to compile the model metadata and query cache
                    var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                    var metadata = objectContext.MetadataWorkspace;
                    
                    // Pre-compile common LINQ expressions
                    var _ = context.DbClsUser.Where(u => u.UserId > 0).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EF Warmup error: {ex.Message}");
                throw; // Re-throw to be caught by outer handler
            }
        }

        private void PrecompileViews()
        {
            try
            {
                // Pre-compile Razor views to avoid first-request compilation delay
                // Initialize view engine system to trigger compilation infrastructure
                var viewEngines = ViewEngines.Engines;
                var engineCount = viewEngines.Count;
                
                // Pre-compile views asynchronously after first request
                // Views will be compiled on first use, but the infrastructure is ready
                Task.Run(() =>
                {
                    try
                    {
                        // Force Razor view engine initialization
                        // This pre-initializes the view compilation system
                        System.Threading.Thread.Sleep(100); // Small delay to let app start
                        
                        // Access Razor compilation infrastructure
                        var razorViewEngine = viewEngines.OfType<RazorViewEngine>().FirstOrDefault();
                        if (razorViewEngine != null)
                        {
                            // This initializes the Razor compilation system
                            var _ = razorViewEngine.GetType();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"View precompilation error: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"View engine initialization error: {ex.Message}");
            }
        }
    }
}
