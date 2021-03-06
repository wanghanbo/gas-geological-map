using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF.BaseClasses;

namespace GIS.GraphicEdit
{
    /// <summary>
    /// 要素选择Menu菜单：选择、全选、清除选择 
    /// </summary>
    [Guid("2efd9371-9458-4ffb-ae42-2f5c9df30dae")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("GIS.GraphicEdit.MenuFeatureSelect")]
    public sealed class MenuFeatureSelect : BaseMenu
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsMenus.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsMenus.Unregister(regKey);
        }

        #endregion
        #endregion

        public MenuFeatureSelect()
        { 
            AddItem("GIS.GraphicEdit.FeatureSelect");
            //AddItem("GIS.GraphicEdit.FeatureSelectAll");
            AddItem("GIS.GraphicEdit.FeatureClearSelect");
        }

        public override string Caption
        {
            get
            {                
                return "选择";
            }
        }
        public override string Name
        {
            get
            {                
                return "MenuFeatureSelect";
            }
        }
    }
}