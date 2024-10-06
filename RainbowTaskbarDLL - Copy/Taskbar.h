#pragma once
#include "winrt.h"

struct Taskbar {
public:
    winrt::Windows::UI::Xaml::Shapes::Rectangle rectangleBackground = nullptr;
    winrt::Windows::UI::Xaml::Shapes::Rectangle border = nullptr;
    winrt::Windows::UI::Xaml::Media::Brush originalBrush = nullptr;
    winrt::Windows::UI::Core::CoreDispatcher dispatcher = nullptr;
    winrt::Windows::UI::Xaml::FrameworkElement gripper = nullptr;
    
    HWND handle = NULL;
    int tInfoIndex = 0;

    Taskbar() {

    }
};