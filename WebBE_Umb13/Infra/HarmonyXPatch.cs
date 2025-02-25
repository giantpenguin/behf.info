using HarmonyLib;
using Umbraco.Licensing;

namespace YeahWeb.Infra;

public class PatchLicense
{
    public static void DoPatch()
    {
        var harmony = new Harmony("AutoPatchLocal");
        // harmony.PatchAll(Assembly.GetExecutingAssembly());

        PatchUmbraco(harmony);
        PatchUsync(harmony);
    }

    public static void PatchUmbraco(Harmony harmony)
    {
        var formLicensingServiceType = AccessTools.TypeByName("Umbraco.Forms.Core.Services.Licensing.LicensingService");
        harmony.Patch(
            formLicensingServiceType.GetMethod("IsTrial"),
            postfix: new HarmonyMethod(typeof(PatchUmbracoLicense).GetMethod(nameof(PatchUmbracoLicense.FormLicensingService_IsTrial))));
        harmony.Patch(
            formLicensingServiceType.GetMethod("IsInvalid"),
            postfix: new HarmonyMethod(typeof(PatchUmbracoLicense).GetMethod(nameof(PatchUmbracoLicense.FormLicensingService_IsTrial))));
        harmony.Patch(
            formLicensingServiceType.GetMethod("ValidDomains"),
            postfix: new HarmonyMethod(typeof(PatchUmbracoLicense).GetMethod(nameof(PatchUmbracoLicense.FormLicensingService_ValidDomains))));


        // var validatorType = AccessTools.TypeByName("Umbraco.Forms.Core.Services.Licensing.FormsValidator");
        // harmony.Patch(
        //     validatorType.GetMethod("GetAllLicenseFiles", new Type[] { typeof(string) }),
        //     postfix: new HarmonyMethod(typeof(PatchUmbracoLicense).GetMethod(nameof(PatchUmbracoLicense.Validator_GetAllLicenseFiles))));
    }

    public static void PatchUsync(Harmony harmony)
    {
        var licenseType = AccessTools.TypeByName("uSync.Expansions.Core.Licencing.Licence");
        harmony.Patch(
            licenseType.GetMethod("IsValid"),
            postfix: new HarmonyMethod(typeof(PatchUsyncLicense).GetMethod(nameof(PatchUsyncLicense.License_IsValid))));
        harmony.Patch(
            licenseType.GetMethod("IsValidLicence"),
            postfix: new HarmonyMethod(typeof(PatchUsyncLicense).GetMethod(nameof(PatchUsyncLicense.License_IsValid))));
        harmony.Patch(
            licenseType.GetProperty("Expiry").GetMethod,
            postfix: new HarmonyMethod(typeof(PatchUsyncLicense).GetMethod(nameof(PatchUsyncLicense.License_Expire))));
        harmony.Patch(
            licenseType.GetProperty("AutoRenews").GetMethod,
            postfix: new HarmonyMethod(typeof(PatchUsyncLicense).GetMethod(nameof(PatchUsyncLicense.License_IsValid))));
        harmony.Patch(
            licenseType.GetMethod("GetStatus"),
            postfix: new HarmonyMethod(typeof(PatchUsyncLicense).GetMethod(nameof(PatchUsyncLicense.License_Status)))
            );

    }

}

#region Umbraco License (for Forms and Deploy)
public class PatchUmbracoLicense
{
    public static void FormLicensingService_IsTrial(ref bool __result)
    {
        __result = false;
    }

    public static void FormLicensingService_IsValid(ref bool __result)
    {
        __result = true;
    }

    public static void FormLicensingService_ValidDomains(ref string __result)
    {
        __result = "*";
    }

    public static void Validator_GetAllLicenseFiles(ref List<EncryptedLicense> __result)
    {
        __result.Add(new EncryptedLicense("UmbracoForms", 123456, @"
                <license>
                    <info>
                        <trial>False</trial>
                        <serialNumber>123456</serialNumber>
                        <productName>umbracoForms</productName>
                        <productVersion>n/a</productVersion>
                        <company>Home</company>
                        <contactName>None</contactName>
                        <orderDate>2020-09-15T14:27:10</orderDate>
                        <orderNumber>0</orderNumber>
                        <invoiceNumber>1209</invoiceNumber>
                    </info>
                    <restrictions>
                        <domains>
                            <domain>*</domain>
                        </domains>
                        <ips />
                    </restrictions>
                    <assets></assets>
                </license>"));
        __result.Add(new EncryptedLicense("UmbracoDeploy", 123456000, @"
                <license>
                    <info>
                        <trial>False</trial>
                        <serialNumber>123456000</serialNumber>
                        <productName>umbracoDeploy</productName>
                        <productVersion>n/a</productVersion>
                        <company>Home</company>
                        <contactName>None</contactName>
                        <orderDate>2020-09-15T14:27:10</orderDate>
                        <orderNumber>0</orderNumber>
                        <invoiceNumber>1209</invoiceNumber>
                    </info>
                    <restrictions>
                        <domains>
                            <domain>*</domain>
                        </domains>
                        <ips />
                    </restrictions>
                    <assets></assets>
                </license>"));
    }
}
#endregion

#region uSync
public class PatchUsyncLicense
{
    public static void License_IsValid(ref bool __result)
    {
        __result = true;
    }

    public static void License_Expire(ref DateTime __result)
    {
        __result = DateTime.Now.AddYears(10);
    }

    public static void License_Status(ref uSync.Expansions.Core.Licencing.LicenceStatus __result)
    {
        __result = uSync.Expansions.Core.Licencing.LicenceStatus.Valid;
    }
}
#endregion
