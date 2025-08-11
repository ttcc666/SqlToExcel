# SQL to Excel UI 优化实施细节

## 第三阶段：界面具体修改方案

### 3.1 主窗口 (MainWindow.xaml) 修改

#### 现有问题：
- 按钮样式不统一
- 间距不一致
- 缺少统一的图标使用

#### 修改方案：
```xml
<!-- 修改前 -->
<Button Content="预览" Command="{Binding Preview1Command}" 
        Style="{StaticResource ButtonPrimary}" 
        hc:IconElement.Geometry="{StaticResource App.Eye}" />

<!-- 修改后 -->
<Button Command="{Binding Preview1Command}" 
        Style="{StaticResource UnifiedButtonStyle}">
    <StackPanel Orientation="Horizontal">
        <Path Data="{StaticResource IconPreview}" 
              Width="16" Height="16" 
              Fill="{Binding RelativeSource={RelativeSource AncestorType=Button}, Path=Foreground}"
              Margin="0,0,5,0"/>
        <TextBlock Text="预览" VerticalAlignment="Center"/>
    </StackPanel>
</Button>
```

### 3.2 数据库配置对话框 (DatabaseConfigView.xaml) 修改

#### 现有问题：
- 使用普通Window而非BlurWindow
- 布局间距不统一
- 按钮样式不一致

#### 修改方案：
```xml
<!-- 修改窗口定义 -->
<hc:BlurWindow x:Class="SqlToExcel.Views.DatabaseConfigView"
               Style="{StaticResource UnifiedWindowStyle}"
               Title="数据库连接配置" 
               Height="300" Width="600">
    
    <Grid Margin="{StaticResource MarginL}">
        <!-- 使用统一的间距 -->
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>
        
        <!-- 使用卡片包装内容 -->
        <hc:Card Grid.Row="0" Style="{StaticResource UnifiedCardStyle}">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="120" />
                    <ColumnDefinition Width="*" />
                    <ColumnDefinition Width="Auto" />
                </Grid.ColumnDefinitions>
                
                <TextBlock Text="源数据库:" 
                          VerticalAlignment="Center"
                          FontWeight="SemiBold"/>
                <hc:TextBox Grid.Column="1" 
                           Text="{Binding SourceConnectionString}"
                           Style="{StaticResource UnifiedTextBoxStyle}"
                           hc:InfoElement.Placeholder="请输入源数据库连接字符串"/>
                <Button Grid.Column="2" 
                       Command="{Binding TestSourceConnectionCommand}"
                       Style="{StaticResource SmallButtonStyle}">
                    <StackPanel Orientation="Horizontal">
                        <Path Data="{StaticResource IconDatabase}" 
                              Width="14" Height="14" 
                              Fill="{Binding RelativeSource={RelativeSource AncestorType=Button}, Path=Foreground}"
                              Margin="0,0,4,0"/>
                        <TextBlock Text="测试连接"/>
                    </StackPanel>
                </Button>
            </Grid>
        </hc:Card>
    </Grid>
</hc:BlurWindow>
```

### 3.3 批量导出视图 (BatchExportView.xaml) 修改

#### 现有问题：
- DataGrid样式未使用统一样式
- 工具栏缺少图标
- 操作按钮布局不美观

#### 修改方案：
```xml
<UserControl x:Class="SqlToExcel.Views.BatchExportView">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- 改进的工具栏 -->
        <hc:Card Grid.Row="0" 
                 Style="{StaticResource UnifiedCardStyle}"
                 Margin="0,0,0,10">
            <StackPanel Orientation="Horizontal">
                <Button Command="{Binding ExportConfigsCommand}"
                        Style="{StaticResource UnifiedButtonStyle}"
                        ToolTip="将所有配置导出到一个JSON文件">
                    <StackPanel Orientation="Horizontal">
                        <Path Data="{StaticResource IconExport}" 
                              Width="16" Height="16" 
                              Fill="{Binding RelativeSource={RelativeSource AncestorType=Button}, Path=Foreground}"
                              Margin="0,0,5,0"/>
                        <TextBlock Text="导出配置"/>
                    </StackPanel>
                </Button>
            </StackPanel>
        </hc:Card>

        <!-- 改进的DataGrid -->
        <DataGrid Grid.Row="1" 
                  ItemsSource="{Binding Items}" 
                  Style="{StaticResource UnifiedDataGridStyle}"
                  ColumnHeaderStyle="{StaticResource UnifiedDataGridColumnHeaderStyle}"
                  CellStyle="{StaticResource UnifiedDataGridCellStyle}"
                  RowStyle="{StaticResource UnifiedDataGridRowStyle}">
            <DataGrid.Columns>
                <!-- 列定义保持不变，但操作列需要改进 -->
                <DataGridTemplateColumn Header="操作" Width="Auto">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <StackPanel Orientation="Horizontal" Margin="5">
                                <Button Command="{Binding DataContext.PreviewCommand, ElementName=dataGrid}"
                                        CommandParameter="{Binding}"
                                        Style="{StaticResource SmallButtonStyle}"
                                        hc:BorderElement.CornerRadius="4"
                                        Background="{StaticResource InfoBrush}"
                                        Margin="0,0,5,0">
                                    <Path Data="{StaticResource IconPreview}" 
                                          Width="14" Height="14" 
                                          Fill="White"/>
                                </Button>
                                <!-- 其他按钮类似 -->
                            </StackPanel>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>
    </Grid>
</UserControl>
```

### 3.4 字段类型提取工具 (FieldTypeExtractorView.xaml) 修改

#### 现有问题：
- 使用内联样式定义
- 颜色硬编码
- 布局不够优雅

#### 修改方案：
```xml
<hc:BlurWindow x:Class="SqlToExcel.Views.FieldTypeExtractorView"
                Style="{StaticResource UnifiedWindowStyle}"
                Title="字段类型提取工具" 
                Height="700" Width="900">
    
    <Grid Margin="{StaticResource MarginL}">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="250" MinHeight="150"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*" MinHeight="200"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- 标题卡片 -->
        <hc:Card Grid.Row="0" Style="{StaticResource UnifiedCardStyle}">
            <StackPanel>
                <TextBlock Text="字段类型提取工具" 
                          FontSize="{StaticResource FontSizeH2}" 
                          FontWeight="SemiBold" 
                          Margin="0,0,0,5"/>
                <TextBlock Text="输入JSON格式的表名和字段列表，获取Target数据库中的字段类型信息" 
                          Foreground="{StaticResource TextSecondaryColor}"
                          FontSize="{StaticResource FontSizeSmall}"/>
            </StackPanel>
        </hc:Card>

        <!-- JSON输入区域 -->
        <hc:Card Grid.Row="2" 
                 Style="{StaticResource UnifiedCardStyle}"
                 Header="JSON输入">
            <hc:TextBox Text="{Binding JsonInput, UpdateSourceTrigger=PropertyChanged}"
                       AcceptsReturn="True"
                       AcceptsTab="True"
                       FontFamily="{StaticResource CodeFontFamily}"
                       Style="{StaticResource UnifiedTextBoxStyle}"
                       hc:InfoElement.Placeholder='{"table": "表名", "fields": ["字段1", "字段2"]}'
                       VerticalScrollBarVisibility="Auto"/>
        </hc:Card>

        <!-- 按钮组 -->
        <StackPanel Grid.Row="3" 
                    Orientation="Horizontal" 
                    HorizontalAlignment="Center"
                    Margin="{StaticResource MarginM}">
            <Button Command="{Binding ExtractFieldTypesCommand}"
                    Style="{StaticResource LargeButtonStyle}"
                    hc:BorderElement.CornerRadius="4">
                <StackPanel Orientation="Horizontal">
                    <Path Data="{StaticResource IconSearch}" 
                          Width="18" Height="18" 
                          Fill="{Binding RelativeSource={RelativeSource AncestorType=Button}, Path=Foreground}"
                          Margin="0,0,8,0"/>
                    <TextBlock Text="获取字段类型" VerticalAlignment="Center"/>
                </StackPanel>
            </Button>
            
            <!-- 其他按钮类似 -->
        </StackPanel>

        <!-- 结果显示区域 -->
        <hc:Card Grid.Row="4" 
                 Style="{StaticResource UnifiedCardStyle}"
                 Header="查询结果">
            <DataGrid ItemsSource="{Binding FieldTypes}"
                      Style="{StaticResource UnifiedDataGridStyle}"
                      ColumnHeaderStyle="{StaticResource UnifiedDataGridColumnHeaderStyle}"
                      CellStyle="{StaticResource UnifiedDataGridCellStyle}"
                      RowStyle="{StaticResource UnifiedDataGridRowStyle}">
                <!-- DataGrid列定义 -->
            </DataGrid>
        </hc:Card>

        <!-- 状态栏 -->
        <Border Grid.Row="5" 
                Background="{StaticResource SurfaceColor}"
                BorderBrush="{StaticResource BorderColor}"
                BorderThickness="0,1,0,0"
                Margin="-20,10,-20,-20">
            <Grid>
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="*"/>
                    <ColumnDefinition Width="Auto"/>
                </Grid.ColumnDefinitions>
                
                <TextBlock Text="{Binding StatusMessage}" 
                          Margin="20,8"
                          VerticalAlignment="Center"/>
                
                <hc:LoadingCircle Grid.Column="1"
                                  IsRunning="{Binding IsProcessing}"
                                  Margin="0,0,20,0"
                                  Width="20" Height="20"/>
            </Grid>
        </Border>
    </Grid>
</hc:BlurWindow>
```

## 第四阶段：创建统一组件库

### 4.1 创建自定义用户控件

#### UnifiedButton.xaml
```xml
<UserControl x:Class="SqlToExcel.Controls.UnifiedButton">
    <Button x:Name="MainButton"
            Style="{StaticResource UnifiedButtonStyle}">
        <StackPanel Orientation="Horizontal">
            <Path x:Name="IconPath" 
                  Width="16" Height="16" 
                  Fill="{Binding RelativeSource={RelativeSource AncestorType=Button}, Path=Foreground}"
                  Margin="0,0,5,0"
                  Visibility="{Binding IconVisibility}"/>
            <TextBlock x:Name="ButtonText" 
                      Text="{Binding Text}"
                      VerticalAlignment="Center"/>
        </StackPanel>
    </Button>
</UserControl>
```

### 4.2 创建统一对话框基类

#### UnifiedDialog.cs
```csharp
public class UnifiedDialog : hc:BlurWindow
{
    public UnifiedDialog()
    {
        this.Style = Application.Current.FindResource("UnifiedWindowStyle") as Style;
        this.ShowInTaskbar = false;
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
    }
    
    protected virtual void OnOkClick()
    {
        this.DialogResult = true;
        this.Close();
    }
    
    protected virtual void OnCancelClick()
    {
        this.DialogResult = false;
        this.Close();
    }
}
```

## 第五阶段：主题切换实现

### 5.1 创建主题服务扩展

```csharp
public class ThemeService
{
    private const string LightTheme = "Light";
    private const string DarkTheme = "Dark";
    private string currentTheme = LightTheme;
    
    public void SwitchTheme()
    {
        currentTheme = currentTheme == LightTheme ? DarkTheme : LightTheme;
        ApplyTheme(currentTheme);
    }
    
    private void ApplyTheme(string themeName)
    {
        var app = Application.Current;
        var themeDict = new ResourceDictionary();
        
        string themeFile = themeName == DarkTheme 
            ? "Resources/DarkTheme.xaml" 
            : "Resources/LightTheme.xaml";
            
        themeDict.Source = new Uri(themeFile, UriKind.Relative);
        
        // 移除旧主题
        var oldTheme = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme.xaml") == true);
        if (oldTheme != null)
            app.Resources.MergedDictionaries.Remove(oldTheme);
            
        // 应用新主题
        app.Resources.MergedDictionaries.Add(themeDict);
        
        // 保存用户偏好
        SaveThemePreference(themeName);
    }
}
```

### 5.2 深色主题资源文件 (Resources/DarkTheme.xaml)

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <!-- 深色主题颜色 -->
    <Color x:Key="BackgroundColor">#1E1E1E</Color>
    <Color x:Key="SurfaceColor">#2D2D2D</Color>
    <Color x:Key="BorderColor">#3C3C3C</Color>
    <Color x:Key="TextPrimaryColor">#E0E0E0</Color>
    <Color x:Key="TextSecondaryColor">#9E9E9E</Color>
    
    <!-- 覆盖画刷定义 -->
    <SolidColorBrush x:Key="BackgroundBrush" Color="{StaticResource BackgroundColor}"/>
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}"/>
    <SolidColorBrush x:Key="BorderBrush" Color="{StaticResource BorderColor}"/>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}"/>
    <SolidColorBrush x:Key="TextSecondaryBrush" Color="{StaticResource TextSecondaryColor}"/>
</ResourceDictionary>
```

## 第六阶段：测试清单

### 6.1 视觉一致性测试
- [ ] 所有窗口使用统一的窗口样式
- [ ] 所有按钮使用统一的按钮样式
- [ ] 所有输入框使用统一的输入框样式
- [ ] 所有卡片使用统一的卡片样式
- [ ] 所有DataGrid使用统一的表格样式
- [ ] 图标风格统一且大小一致
- [ ] 间距和边距保持一致
- [ ] 字体大小和字重符合规范

### 6.2 交互一致性测试
- [ ] Tab键顺序合理
- [ ] 快捷键功能正常
- [ ] 焦点管理正确
- [ ] 加载状态显示正常
- [ ] 错误提示显示正常
- [ ] 禁用状态显示正常

### 6.3 主题切换测试
- [ ] 浅色主题显示正常
- [ ] 深色主题显示正常
- [ ] 主题切换平滑无闪烁
- [ ] 主题偏好保存成功
- [ ] 所有控件在两种主题下都清晰可见

### 6.4 响应式测试
- [ ] 窗口缩放正常
- [ ] 最小窗口尺寸限制有效
- [ ] 不同分辨率下显示正常
- [ ] 控件自适应布局正常

## 第七阶段：性能优化

### 7.1 资源优化
- 合并相似样式，减少重复定义
- 使用静态资源而非动态资源
- 延迟加载非必要资源

### 7.2 渲染优化
- 使用虚拟化技术处理大量数据
- 减少不必要的动画效果
- 优化复杂控件模板

### 7.3 内存优化
- 及时释放不使用的资源
- 避免内存泄漏
- 使用弱引用处理事件订阅

## 实施时间表

| 阶段 | 任务 | 预计时间 | 优先级 |
|------|------|----------|--------|
| 1 | 创建资源文件 | 2小时 | 高 |
| 2 | 修改App.xaml | 0.5小时 | 高 |
| 3 | 修改主窗口 | 1小时 | 高 |
| 4 | 修改对话框 | 3小时 | 中 |
| 5 | 修改视图 | 3小时 | 中 |
| 6 | 创建组件库 | 2小时 | 低 |
| 7 | 实现主题切换 | 2小时 | 低 |
| 8 | 测试和调试 | 2小时 | 高 |
| 9 | 性能优化 | 1小时 | 低 |

总计预计时间：16.5小时

## 注意事项

1. **向后兼容**：确保修改不影响现有功能
2. **渐进式更新**：可以分批次实施，优先处理用户最常用的界面
3. **用户反馈**：实施过程中收集用户反馈，及时调整
4. **文档更新**：同步更新用户手册和开发文档
5. **版本控制**：使用Git分支进行开发，确保可以回滚

## 后续维护

1. 定期审查UI一致性
2. 收集用户体验反馈
3. 跟踪新的设计趋势
4. 持续优化性能
5. 扩展主题选项