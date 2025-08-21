#pragma once
#include "winrt.h"

struct UITree {
    winrt::Windows::UI::Xaml::UIElement element{ nullptr };
    std::vector<UITree> children;
};

struct Taskbar {
public:
    winrt::Windows::UI::Xaml::Shapes::Rectangle rectangleBackground = nullptr;
    winrt::Windows::UI::Xaml::Shapes::Rectangle border = nullptr;
    winrt::Windows::UI::Xaml::Media::Brush originalBrush = nullptr;
    winrt::Windows::UI::Core::CoreDispatcher dispatcher = nullptr;
    winrt::Windows::UI::Xaml::FrameworkElement gripper = nullptr;
    winrt::Windows::UI::Xaml::FrameworkElement frame = nullptr;
    winrt::Windows::UI::Xaml::FrameworkElement SystemTrayFrame = nullptr;
    winrt::Windows::UI::Xaml::FrameworkElement TaskbarFrameRepeater = nullptr;
    
    HWND handle = NULL;
    int tInfoIndex = 0;

    Taskbar() {

    }
};