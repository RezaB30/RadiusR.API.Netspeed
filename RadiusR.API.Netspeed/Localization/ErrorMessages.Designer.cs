﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RadiusR.API.Netspeed.Localization {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ErrorMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RadiusR.API.Netspeed.Localization.ErrorMessages", typeof(ErrorMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Authentication Failed.
        /// </summary>
        public static string AuthenticationFailed {
            get {
                return ResourceManager.GetString("AuthenticationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t be empty bill id list.
        /// </summary>
        public static string BillsNotFound {
            get {
                return ResourceManager.GetString("BillsNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed.
        /// </summary>
        public static string Failed {
            get {
                return ResourceManager.GetString("Failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Customer has more subscription..
        /// </summary>
        public static string HasMoreSubscription {
            get {
                return ResourceManager.GetString("HasMoreSubscription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Internal Server Error.
        /// </summary>
        public static string InternalServerError {
            get {
                return ResourceManager.GetString("InternalServerError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Null object error.
        /// </summary>
        public static string NullObjectFound {
            get {
                return ResourceManager.GetString("NullObjectFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subscriber not found.
        /// </summary>
        public static string SubscriberNotFound {
            get {
                return ResourceManager.GetString("SubscriberNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Success.
        /// </summary>
        public static string Success {
            get {
                return ResourceManager.GetString("Success", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to have wrong or invalid Bills.
        /// </summary>
        public static string WrongOrInvalidBill {
            get {
                return ResourceManager.GetString("WrongOrInvalidBill", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SMS password is wrong.
        /// </summary>
        public static string WrongSMSPassword {
            get {
                return ResourceManager.GetString("WrongSMSPassword", resourceCulture);
            }
        }
    }
}
