// dllmain.cpp : Defines the entry point for the DLL application.
#include "framework.h"
#include "winrt.h"
#include "TAP.h"
#include "RainbowTaskbarDLL_h.h"
#include "Factory.h"


_Check_return_ STDAPI OldDllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _Outptr_ LPVOID* ppv);

#include <comdef.h>
#include <tchar.h>
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID* ppv) try
{
    if (rclsid == CLSID_TAPSite) {
        return winrt::make<Factory<TAP>>().as(riid, ppv);
    }
    else {
        return OldDllGetClassObject(rclsid, riid, ppv);;
    }
}
catch (...)
{
    HRESULT res = winrt::to_hresult();
    _com_error err(res);
    MessageBox(0, err.ErrorMessage(), _T("DllGetClassObject RainbowTaskbarDLL Error"), 0);
    return res;
}

STDAPI DllCanUnloadNow()
{
    return S_OK;
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
   
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        break;
    case DLL_THREAD_ATTACH:

        break;
    case DLL_THREAD_DETACH:

        break;
    case DLL_PROCESS_DETACH:
        AppearanceServiceAPI::Unload();
        break;
    }
    return TRUE;
}

