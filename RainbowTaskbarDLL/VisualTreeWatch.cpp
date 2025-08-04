
#include "VisualTreeWatch.h"
#include "winrt.h"
#include <winrt/Windows.UI.Xaml.Hosting.h>
#include <windows.ui.xaml.hosting.desktopwindowxamlsource.h>
#include <winrt/Windows.UI.Xaml.h>
#include <winrt/Windows.UI.Xaml.Shapes.h>
#include <winrt/Windows.UI.Xaml.Media.h>
#include <winrt/Windows.UI.Core.h>

#include <comdef.h>
#include <tchar.h>
#include <inspectable.h>
#include <winstring.h>
#include <fstream>
#include <winrt/base.h>

winrt::Windows::UI::Xaml::FrameworkElement FindRootElement(winrt::Windows::UI::Xaml::FrameworkElement child)
{
    const auto parent = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetParent(child).try_as<winrt::Windows::UI::Xaml::FrameworkElement>();
    if (parent)
    {
        return FindRootElement(parent);
    }
    else
    {
        return child;
    }
    
}

std::vector<std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>> VisualTreeWatch::FindChildrenRecursive(std::optional<std::wstring> name, winrt::Windows::UI::Xaml::FrameworkElement root, int deep = 0)
try {
    auto children = std::vector<std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>>();
  
    auto childrenc = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetChildrenCount(root);
    for (int i = 0; i < childrenc; i++) {
        auto child_ = winrt::Windows::UI::Xaml::Media::VisualTreeHelper::GetChild(root, i);
        auto child = child_.try_as< winrt::Windows::UI::Xaml::FrameworkElement>();
        if (!name.has_value() || child.Name() == name.value()) {
            children.push_back(std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>(deep, child));
        }
        auto recurse = FindChildrenRecursive(name, child, deep+1);
        children.insert(children.end(), recurse.begin(), recurse.end());
        
  
    }
    return children;
}
catch (...) {
    return std::vector<std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>>();
}

std::vector<std::pair<int, winrt::Windows::UI::Xaml::FrameworkElement>> VisualTreeWatch::FindAllChildren(winrt::Windows::UI::Xaml::FrameworkElement root)
{
    return FindChildrenRecursive(std::nullopt, root);
}

winrt::Windows::Foundation::IAsyncAction StartYPosTask(Taskbar* taskbar, VisualTreeWatch* vtw) {
    vtw->data->lTaskbarInfo[taskbar->tInfoIndex]._reserved[0] = 1;
    co_await winrt::resume_background();
    co_await winrt::resume_after(std::chrono::seconds(1));
    while (true) {
        co_await winrt::resume_foreground(vtw->dispatcher);
        auto v = taskbar->gripper.TransformToVisual(vtw->root).TransformPoint(winrt::Windows::Foundation::Point(0, 0)).Y;
        co_await winrt::resume_background();
        vtw->data->lTaskbarInfo[taskbar->tInfoIndex].YPosition = v;
        co_await winrt::resume_after(std::chrono::milliseconds(16));
    }
}


HRESULT VisualTreeWatch::OnVisualTreeChange(ParentChildRelation rel, VisualElement element, VisualMutationType mutationType)
{_F
    try {
        // Navigate down the visual tree and find the taskbarframes
    // * this is required because we cannot get the DesktopWindowXamlSource by navigating up the tree

    // we also cannot get its children so we wait for an element that has it as its parent, 
    // its weird idk
_E      auto changed = ConvertFromH< winrt::Windows::UI::Xaml::Hosting::IDesktopWindowXamlSource>(rel.Parent);
_E      if (changed && mutationType == Add) {
_E          root = ConvertFromH<winrt::Windows::UI::Xaml::FrameworkElement>(element.Handle);
_E          if (root) {
_E
_E              auto children = FindChildrenRecursive(L"TaskbarFrame", root);
_E              for (auto& frame : children) {
_E                  InstanceHandle handle = ConvertToH(frame.second);
_E                  
_E
_E                  auto rects = FindChildrenRecursive(L"BackgroundFill", frame.second);
_E                  for (auto& rect_ : rects) {
_E                      auto rect = rect_.second.try_as<winrt::Windows::UI::Xaml::Shapes::Rectangle>();
_E                      if (rect) {
_E                          
_E                          if (taskbarMap.find(handle) == taskbarMap.end()) {
_E                              Taskbar t = Taskbar();
_E                              t.originalBrush = rect.Fill();
_E                              t.rectangleBackground = rect;
_E                              t.dispatcher = rect.Dispatcher();
_E                              HWND hwnd;
_E                              changed.try_as<IDesktopWindowXamlSourceNative>()->get_WindowHandle(&hwnd);
_E                              t.handle = GetParent(hwnd);
_E                              for (int i = 0; i < 8; i++) {
_E                                  if (!data->lTaskbarInfo[i].taskbar) {
_E                                      data->lTaskbarInfo[i].taskbar = t.handle;
_E                                      t.tInfoIndex = i;
_E                                      break;
_E                                  }
_E                              }
_E
_E
_E                              taskbarMap.emplace(handle, t);
_E                          }
_E                      }
_E                  }
_E
_E                  auto borders = FindChildrenRecursive(L"BackgroundStroke", frame.second);
_E                  for (auto& border_ : borders) {
_E                      auto border = border_.second.try_as<winrt::Windows::UI::Xaml::Shapes::Rectangle>();
_E                      if (border) {
_E                          if(taskbarMap.find(handle) != taskbarMap.end()) 
                                taskbarMap.find(handle)->second.border = border;
_E                      }
_E                  }
_E
_E                  auto gripper = FindChildrenRecursive(L"GripperControl", frame.second);
_E                  if (gripper.begin()->second) {
_E                      auto grip = gripper.begin()->second;
_E                      if(taskbarMap.find(handle) != taskbarMap.end()) {
_E                          auto& tb = taskbarMap.find(handle)->second;
_E                          tb.gripper = grip;
_E                          StartYPosTask(&taskbarMap.find(handle)->second, this);
_E                          
_E                      
_E                      }
_E                  }
_E
_E              }
_E          }
_E      }
        

    }
    catch (...) {
        HRESULT res = winrt::to_hresult();
        _com_error err(res);
        WCHAR data[1024];
        __EFMT(data, err.ErrorMessage());
        return res;
    }
    
    return S_OK;
}

void VisualTreeWatch::Stop() {
    xamlDiagnostics.as<IVisualTreeService3>()->UnadviseVisualTreeChange(this);
}


HRESULT VisualTreeWatch::OnElementStateChanged(InstanceHandle, VisualElementState, LPCWSTR) noexcept
{
    return S_OK;
}


VisualTreeWatch::VisualTreeWatch() {
    appService = winrt::make_self<AppearanceServiceAPI>(get_strong());


}