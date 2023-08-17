#pragma once
#include <Unknwn.h>
#include "winrt.h"

template<class T>
struct Factory : winrt::implements<Factory<T>, IClassFactory>
{
    HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppvObject) override try
    {
        *ppvObject = nullptr;
        return winrt::make<T>().as(riid, ppvObject);
    }
    catch (...)
    {
        return winrt::to_hresult();
    }

    HRESULT STDMETHODCALLTYPE LockServer(BOOL fLock) noexcept override
    {
        if (fLock)
        {
            ++winrt::get_module_lock();
        }
        else
        {
            --winrt::get_module_lock();
        }

        return S_OK;
    }
};