import tkinter as tk
from tkinter import messagebox

class App:
    def __init__(self, root):
        # 基础窗口设置
        self.root = root
        self.root.title("跨平台 Tkinter 示例")
        self.root.geometry("400x300")  # 窗口初始大小
        
        # 确保中文显示正常（macOS 默认支持中文字体）
        self.default_font = ('Helvetica', 12)
        
        # 创建菜单栏（macOS 上会显示在顶部系统菜单）
        self.create_menu()
        
        # 创建主界面组件
        self.create_widgets()
        
    def create_menu(self):
        menubar = tk.Menu(self.root)
        
        # 文件菜单
        file_menu = tk.Menu(menubar, tearoff=0)
        file_menu.add_command(label="打开", command=self.on_open, accelerator="Command+O")
        file_menu.add_command(label="保存", command=self.on_save, accelerator="Command+S")
        file_menu.add_separator()
        file_menu.add_command(label="退出", command=self.on_quit, accelerator="Command+Q")
        menubar.add_cascade(label="文件", menu=file_menu)
        
        # 帮助菜单
        help_menu = tk.Menu(menubar, tearoff=0)
        help_menu.add_command(label="关于", command=self.on_about)
        menubar.add_cascade(label="帮助", menu=help_menu)
        
        # 将菜单栏应用到窗口
        self.root.config(menu=menubar)
        
        # 为 macOS 配置快捷键命令键
        if self.is_mac():
            self.root.bind("<Command-o>", lambda e: self.on_open())
            self.root.bind("<Command-s>", lambda e: self.on_save())
            self.root.bind("<Command-q>", lambda e: self.on_quit())
    
    def create_widgets(self):
        # 主框架
        main_frame = tk.Frame(self.root, padx=20, pady=20)
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # 标题标签
        title_label = tk.Label(
            main_frame, 
            text="Tkinter 跨平台示例", 
            font=('Helvetica', 16, 'bold')
        )
        title_label.pack(pady=(0, 20))
        
        # 输入框和标签
        input_frame = tk.Frame(main_frame)
        input_frame.pack(fill=tk.X, pady=10)
        
        tk.Label(input_frame, text="输入内容:", font=self.default_font).pack(side=tk.LEFT, padx=(0, 10))
        
        self.input_entry = tk.Entry(
            input_frame, 
            font=self.default_font, 
            width=25,
            bg="white",
            fg="black"
        )
        self.input_entry.pack(side=tk.LEFT, fill=tk.X, expand=True)
        self.input_entry.focus_set()
        
        # 调试输出
        print(f"输入框位置: {self.input_entry.winfo_x()}, {self.input_entry.winfo_y()}")
        print(f"输入框大小: {self.input_entry.winfo_width()} x {self.input_entry.winfo_height()}")
        
        # 按钮
        button_frame = tk.Frame(main_frame)
        button_frame.pack(fill=tk.X, pady=20)
        
        self.submit_button = tk.Button(
            button_frame, 
            text="提交", 
            font=self.default_font,
            command=self.on_submit
        )
        self.submit_button.pack(side=tk.LEFT, padx=(0, 10))
        
        self.clear_button = tk.Button(
            button_frame, 
            text="清空", 
            font=self.default_font,
            command=self.on_clear
        )
        self.clear_button.pack(side=tk.LEFT)
        
        # 结果显示区域
        self.result_text = tk.Text(
            main_frame, 
            font=self.default_font, 
            height=5, 
            width=40,
            wrap=tk.WORD  # 自动换行
        )
        self.result_text.pack(fill=tk.BOTH, expand=True, pady=10)
        self.result_text.config(state=tk.DISABLED)  # 只读模式
    
    def on_submit(self):
        text = self.input_entry.get()
        if text:
            self.result_text.config(state=tk.NORMAL)
            self.result_text.delete(1.0, tk.END)
            self.result_text.insert(tk.END, f"你输入的内容是：\n{text}")
            self.result_text.config(state=tk.DISABLED)
        else:
            messagebox.showwarning("警告", "请输入内容！")
    
    def on_clear(self):
        self.input_entry.delete(0, tk.END)
        self.result_text.config(state=tk.NORMAL)
        self.result_text.delete(1.0, tk.END)
        self.result_text.config(state=tk.DISABLED)
        self.input_entry.focus_set()
    
    def on_open(self):
        messagebox.showinfo("提示", "打开文件功能已触发")
    
    def on_save(self):
        messagebox.showinfo("提示", "保存文件功能已触发")
    
    def on_quit(self):
        if messagebox.askyesno("确认", "确定要退出吗？"):
            self.root.destroy()
    
    def on_about(self):
        messagebox.showinfo(
            "关于", 
            "Tkinter 跨平台示例程序\n\n"
            "兼容 macOS 和 Windows\n"
            "Python 3.x 版本"
        )
    
    def is_mac(self):
        return self.root.tk.call('tk', 'windowingsystem') == 'aqua'

if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()