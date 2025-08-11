# SQL to Excel UI 优化实施计划

## 概述
本文档详细说明了如何实施UI统一优化，包括具体的代码修改和资源文件创建。

## 第一阶段：创建基础资源文件

### 1.1 创建统一样式资源字典 (Resources/UnifiedStyles.xaml)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:hc="https://handyorg.github.io/handycontrol">

    <!-- ========== 颜色定义 ========== -->
    
    <!-- 主题色 -->
    <Color x:Key="PrimaryColor">#2196F3</Color>
    <Color x:Key="SecondaryColor">#4CAF50</Color>
    <Color x:Key="AccentColor">#FF9800</Color>
    <Color x:Key="DangerColor">#F44336</Color>
    <Color x:Key="WarningColor">#FFC107</Color>
    <Color x:Key="InfoColor">#00BCD4</Color>
    <Color x:Key="SuccessColor">#4CAF50</Color>
    
    <!-- 中性色 -->
    <Color x:Key="BackgroundColor">#FFFFFF</Color>
    <Color x:Key="SurfaceColor">#F5F5F5</Color>
    <Color x:Key="BorderColor">#E0E0E0</Color>
    <Color x:Key="TextPrimaryColor">#212121</Color>
    <Color x:Key="TextSecondaryColor">#757575</Color>
    
    <!-- 画刷定义 -->
    <SolidColorBrush x:Key="PrimaryBrush" Color="{StaticResource PrimaryColor}"/>
    <SolidColorBrush x:Key="SecondaryBrush" Color="{StaticResource SecondaryColor}"/>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}"/>
    <SolidColorBrush x:Key="DangerBrush" Color="{StaticResource DangerColor}"/>
    <SolidColorBrush x:Key="WarningBrush" Color="{StaticResource WarningColor}"/>
    <SolidColorBrush x:Key="InfoBrush" Color="{StaticResource InfoColor}"/>
    <SolidColorBrush x:Key="SuccessBrush" Color="{StaticResource SuccessColor}"/>
    
    <!-- ========== 间距定义 ========== -->
    <Thickness x:Key="MarginXS">5</Thickness>
    <Thickness x:Key="MarginS">10</Thickness>
    <Thickness x:Key="MarginM">15</Thickness>
    <Thickness x:Key="MarginL">20</Thickness>
    <Thickness x:Key="MarginXL">30</Thickness>
    
    <Thickness x:Key="PaddingXS">5</Thickness>
    <Thickness x:Key="PaddingS">8</Thickness>
    <Thickness x:Key="PaddingM">12</Thickness>
    <Thickness x:Key="PaddingL">16</Thickness>
    <Thickness x:Key="PaddingXL">20</Thickness>
    
    <!-- ========== 字体定义 ========== -->
    <FontFamily x:Key="DefaultFontFamily">Microsoft YaHei UI, Segoe UI</FontFamily>
    <FontFamily x:Key="CodeFontFamily">Consolas, Courier New</FontFamily>
    
    <system:Double x:Key="FontSizeH1">24</system:Double>
    <system:Double x:Key="FontSizeH2">20</system:Double>
    <system:Double x:Key="FontSizeH3">18</system:Double>
    <system:Double x:Key="FontSizeH4">16</system:Double>
    <system:Double x:Key="FontSizeBody">14</system:Double>
    <system:Double x:Key="FontSizeSmall">12</system:Double>
    
    <!-- ========== 统一窗口样式 ========== -->
    <Style x:Key="UnifiedWindowStyle" TargetType="hc:BlurWindow">
        <Setter Property="FontFamily" Value="{StaticResource DefaultFontFamily}"/>
        <Setter Property="FontSize" Value="{StaticResource FontSizeBody}"/>
        <Setter Property="Background" Value="{StaticResource BackgroundColor}"/>
        <Setter Property="WindowStartupLocation" Value="CenterOwner"/>
    </Style>
    
    <!-- ========== 统一按钮样式 ========== -->
    <Style x:Key="UnifiedButtonStyle" TargetType="Button" BasedOn="{StaticResource ButtonPrimary}">
        <Setter Property="Height" Value="32"/>
        <Setter Property="MinWidth" Value="100"/>
        <Setter Property="Margin" Value="{StaticResource MarginXS}"/>
        <Setter Property="Padding" Value="12,6"/>
        <Setter Property="FontSize" Value="{StaticResource FontSizeBody}"/>
    </Style>
    
    <Style x:Key="LargeButtonStyle" TargetType="Button" BasedOn="{StaticResource UnifiedButtonStyle}">
        <Setter Property="Height" Value="40"/>
        <Setter Property="MinWidth" Value="120"/>
        <Setter Property="Padding" Value="16,8"/>
    </Style>
    
    <Style x:Key="SmallButtonStyle" TargetType="Button" BasedOn="{StaticResource UnifiedButtonStyle}">
        <Setter Property="Height" Value="28"/>
        <Setter Property="MinWidth" Value="80"/>
        <Setter Property="Padding" Value="10,4"/>
        <Setter Property="FontSize" Value="{StaticResource FontSizeSmall}"/>
    </Style>
    
    <!-- ========== 统一输入框样式 ========== -->
    <Style x:Key="UnifiedTextBoxStyle" TargetType="hc:TextBox">
        <Setter Property="Margin" Value="{StaticResource MarginXS}"/>
        <Setter Property="Padding" Value="{StaticResource PaddingS}"/>
        <Setter Property="FontSize" Value="{StaticResource FontSizeBody}"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>
    </Style>
    
    <!-- ========== 统一卡片样式 ========== -->
    <Style x:Key="UnifiedCardStyle" TargetType="hc:Card">
        <Setter Property="Margin" Value="{StaticResource MarginS}"/>
        <Setter Property="Padding" Value="{StaticResource PaddingM}"/>
        <Setter Property="Effect" Value="{StaticResource EffectShadow2}"/>
        <Setter Property="Background" Value="{StaticResource SurfaceColor}"/>
    </Style>
    
    <!-- ========== 统一DataGrid样式 ========== -->
    <Style x:Key="UnifiedDataGridStyle" TargetType="DataGrid">
        <Setter Property="AutoGenerateColumns" Value="False"/>
        <Setter Property="CanUserAddRows" Value="False"/>
        <Setter Property="CanUserDeleteRows" Value="False"/>
        <Setter Property="GridLinesVisibility" Value="Horizontal"/>
        <Setter Property="HeadersVisibility" Value="Column"/>
        <Setter Property="AlternatingRowBackground" Value="#FAFAFA"/>
        <Setter Property="RowHeight" Value="35"/>
        <Setter Property="ColumnHeaderHeight" Value="40"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="BorderBrush" Value="{StaticResource BorderColor}"/>
        <Setter Property="Background" Value="White"/>
        <Setter Property="FontSize" Value="{StaticResource FontSizeBody}"/>
    </Style>
    
    <Style x:Key="UnifiedDataGridColumnHeaderStyle" TargetType="DataGridColumnHeader">
        <Setter Property="Background" Value="{StaticResource PrimaryBrush}"/>
        <Setter Property="Foreground" Value="White"/>
        <Setter Property="FontWeight" Value="SemiBold"/>
        <Setter Property="Height" Value="40"/>
        <Setter Property="HorizontalContentAlignment" Value="Center"/>
        <Setter Property="VerticalContentAlignment" Value="Center"/>
        <Setter Property="Padding" Value="{StaticResource PaddingS}"/>
        <Setter Property="BorderBrush" Value="{StaticResource BorderColor}"/>
        <Setter Property="BorderThickness" Value="0,0,1,0"/>
    </Style>
    
    <Style x:Key="UnifiedDataGridCellStyle" TargetType="DataGridCell">
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Padding" Value="{StaticResource PaddingS}"/>
        <Setter Property="VerticalAlignment" Value="Center"/>
        <Style.Triggers>
            <Trigger Property="IsSelected" Value="True">
                <Setter Property="Background" Value="#E3F2FD"/>
                <Setter Property="Foreground" Value="{StaticResource TextPrimaryColor}"/>
                <Setter Property="BorderBrush" Value="Transparent"/>
            </Trigger>
        </Style.Triggers>
    </Style>
    
    <Style x:Key="UnifiedDataGridRowStyle" TargetType="DataGridRow">
        <Setter Property="MinHeight" Value="35"/>
        <Style.Triggers>
            <Trigger Property="IsMouseOver" Value="True">
                <Setter Property="Background" Value="#F5F5F5"/>
            </Trigger>
            <Trigger Property="AlternationIndex" Value="1">
                <Setter Property="Background" Value="#FAFAFA"/>
            </Trigger>
        </Style.Triggers>
    </Style>
</ResourceDictionary>
```

### 1.2 创建图标资源字典 (Resources/Icons.xaml)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- 数据库图标 -->
    <Geometry x:Key="IconDatabase">M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3M4,9V12C4,14.21 7.58,16 12,16C16.42,16 20,14.21 20,12V9C20,11.21 16.42,13 12,13C7.58,13 4,11.21 4,9M4,14V17C4,19.21 7.58,21 12,21C16.42,21 20,19.21 20,17V14C20,16.21 16.42,18 12,18C7.58,18 4,16.21 4,14Z</Geometry>
    
    <!-- 导出图标 -->
    <Geometry x:Key="IconExport">M23,12L19,8V11H10V13H19V16M1,18V6C1,4.89 1.9,4 3,4H15A2,2 0 0,1 17,6V9H15V6H3V18H15V15H17V18A2,2 0 0,1 15,20H3A2,2 0 0,1 1,18Z</Geometry>
    
    <!-- 导入图标 -->
    <Geometry x:Key="IconImport">M14,12L10,8V11H2V13H10V16M20,18V6C20,4.89 19.1,4 18,4H6A2,2 0 0,0 4,6V9H6V6H18V18H6V15H4V18A2,2 0 0,0 6,20H18A2,2 0 0,0 20,18Z</Geometry>
    
    <!-- 保存图标 -->
    <Geometry x:Key="IconSave">M15,9H5V5H15M12,19A3,3 0 0,1 9,16A3,3 0 0,1 12,13A3,3 0 0,1 15,16A3,3 0 0,1 12,19M17,3H5C3.89,3 3,3.9 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V7L17,3Z</Geometry>
    
    <!-- 删除图标 -->
    <Geometry x:Key="IconDelete">M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z</Geometry>
    
    <!-- 预览图标 -->
    <Geometry x:Key="IconPreview">M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z</Geometry>
    
    <!-- 设置图标 -->
    <Geometry x:Key="IconSettings">M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.21,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.21,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.67 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z</Geometry>
    
    <!-- 刷新图标 -->
    <Geometry x:Key="IconRefresh">M17.65,6.35C16.2,4.9 14.21,4 12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20C15.73,20 18.84,17.45 19.73,14H17.65C16.83,16.33 14.61,18 12,18A6,6 0 0,1 6,12A6,6 0 0,1 12,6C13.66,6 15.14,6.69 16.22,7.78L13,11H20V4L17.65,6.35Z</Geometry>
    
    <!-- 添加图标 -->
    <Geometry x:Key="IconAdd">M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z</Geometry>
    
    <!-- 编辑图标 -->
    <Geometry x:Key="IconEdit">M20.71,7.04C21.1,6.65 21.1,6 20.71,5.63L18.37,3.29C18,2.9 17.35,2.9 16.96,3.29L15.12,5.12L18.87,8.87M3,17.25V21H6.75L17.81,9.93L14.06,6.18L3,17.25Z</Geometry>
    
    <!-- 复制图标 -->
    <Geometry x:Key="IconCopy">M19,21H8V7H19M19,5H8A2,2 0 0,0 6,7V21A2,2 0 0,0 8,23H19A2,2 0 0,0 21,21V7A2,2 0 0,0 19,5M16,1H4A2,2 0 0,0 2,3V17H4V3H16V1Z</Geometry>
    
    <!-- 排序图标 -->
    <Geometry x:Key="IconSort">M3,13H15V11H3M3,6V8H21V6M3,18H9V16H3V18Z</Geometry>
    
    <!-- 筛选图标 -->
    <Geometry x:Key="IconFilter">M14,12V19.88C14.04,20.18 13.94,20.5 13.71,20.71C13.32,21.1 12.69,21.1 12.3,20.71L10.29,18.7C10.06,18.47 9.96,18.16 10,17.87V12H9.97L4.21,4.62C3.87,4.19 3.95,3.56 4.38,3.22C4.57,3.08 4.78,3 5,3V3H19V3C19.22,3 19.43,3.08 19.62,3.22C20.05,3.56 20.13,4.19 19.79,4.62L14.03,12H14Z</Geometry>
    
    <!-- 搜索图标 -->
    <Geometry x:Key="IconSearch">M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z</Geometry>
    
    <!-- Excel图标 -->
    <Geometry x:Key="IconExcel">M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20M8,15.5L9.5,17.5L11,14.5L12.5,17.5L14,15.5L15.5,18.5L14,20.5L12.5,17.5L11,20.5L9.5,17.5L8,19.5L6.5,16.5L8,15.5Z</Geometry>
    
    <!-- 主题图标 -->
    <Geometry x:Key="IconTheme">M12,18C11,18 10,17 10,16C10,15 11,14 12,14A2,2 0 0,1 14,16A2,2 0 0,1 12,18M12,11A2,2 0 0,0 10,9A2,2 0 0,0 8,11A2,2 0 0,0 10,13A2,2 0 0,0 12,11M12,4A2,2 0 0,1 14,6A2,2 0 0,1 12,8C11,8 10,7 10,6C10,5 11,4 12,4M20,12A8,8 0 0,0 12,4A8,8 0 0,0 4,12A8,8 0 0,0 12,20A8,8 0 0,0 20,12M22,12A10,10 0 0,1 12,22A10,10 0 0,1 2,12A10,10 0 0,1 12,2A10,10 0 0,1 22,12Z</Geometry>
</ResourceDictionary>
```

## 第二阶段：修改App.xaml

### 2.1 更新App.xaml资源引用

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- HandyControl 主题 -->
            <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml" />
            <ResourceDictionary Source="pack://application:,,,/HandyControl;component/Themes/Theme.xaml" />
            
            <!-- 自定义资源 -->
            <ResourceDictionary Source="Resources/UnifiedStyles.xaml" />
            <ResourceDictionary Source="Resources