import sys
import json
from PyQt5.QtWidgets import (QApplication, QMainWindow, QWidget, QVBoxLayout, 
                           QHBoxLayout, QLabel, QLineEdit, QTextEdit, 
                           QSpinBox, QPushButton, QMessageBox, QListWidget,
                           QListWidgetItem, QSplitter)
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QFont, QPalette, QColor

class CardInfoWindow(QMainWindow):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("卡片信息生成器")
        self.setMinimumSize(1500, 900)
        
        # 设置窗口样式
        self.setStyleSheet("""
            QMainWindow {
                background-color: #f0f0f0;
            }
            QLabel {
                font-size: 24px;
                color: #333;
            }
            QLineEdit, QTextEdit, QSpinBox {
                padding: 15px;
                border: 1px solid #ddd;
                border-radius: 6px;
                background-color: white;
                font-size: 22px;
            }
            QPushButton {
                padding: 18px 36px;
                background-color: #4CAF50;
                color: white;
                border: none;
                border-radius: 6px;
                font-size: 24px;
            }
            QPushButton:hover {
                background-color: #45a049;
            }
            QListWidget {
                font-size: 22px;
                border: 1px solid #ddd;
                border-radius: 6px;
                background-color: white;
            }
            QListWidget::item {
                padding: 12px;
            }
            QListWidget::item:selected {
                background-color: #e0e0e0;
            }
        """)

        # 创建主窗口部件和布局
        main_widget = QWidget()
        self.setCentralWidget(main_widget)
        layout = QHBoxLayout(main_widget)
        layout.setSpacing(22)
        layout.setContentsMargins(30, 30, 30, 30)

        # 创建左侧预览区域
        left_widget = QWidget()
        left_layout = QVBoxLayout(left_widget)
        
        # 添加预览列表
        preview_label = QLabel("卡片列表预览:")
        self.preview_list = QListWidget()
        self.preview_list.itemClicked.connect(self.load_card_data)
        left_layout.addWidget(preview_label)
        left_layout.addWidget(self.preview_list)

        # 创建右侧输入区域
        right_widget = QWidget()
        right_layout = QVBoxLayout(right_widget)
        right_layout.setSpacing(15)

        # 创建输入字段
        self.create_input_fields(right_layout)
        
        # 创建按钮
        self.create_buttons(right_layout)

        # 使用QSplitter来分割左右区域
        splitter = QSplitter(Qt.Horizontal)
        splitter.addWidget(left_widget)
        splitter.addWidget(right_widget)
        splitter.setStretchFactor(0, 1)
        splitter.setStretchFactor(1, 1)
        layout.addWidget(splitter)

        # 存储所有卡片数据
        self.cards_data = []

    def create_input_fields(self, layout):
        # ID输入
        id_layout = QHBoxLayout()
        id_label = QLabel("ID:")
        self.id_input = QLineEdit()
        id_layout.addWidget(id_label)
        id_layout.addWidget(self.id_input)
        layout.addLayout(id_layout)

        # 名称输入
        name_layout = QHBoxLayout()
        name_label = QLabel("名称:")
        self.name_input = QLineEdit()
        name_layout.addWidget(name_label)
        name_layout.addWidget(self.name_input)
        layout.addLayout(name_layout)

        # 符号输入
        symbol_layout = QHBoxLayout()
        symbol_label = QLabel("符号:")
        self.symbol_input = QLineEdit()
        symbol_layout.addWidget(symbol_label)
        symbol_layout.addWidget(self.symbol_input)
        layout.addLayout(symbol_layout)

        # 描述输入
        desc_layout = QVBoxLayout()
        desc_label = QLabel("描述:")
        self.desc_input = QTextEdit()
        self.desc_input.setMaximumHeight(150)
        desc_layout.addWidget(desc_label)
        desc_layout.addWidget(self.desc_input)
        layout.addLayout(desc_layout)

        # 生命值输入
        life_layout = QHBoxLayout()
        life_label = QLabel("生命值:")
        self.life_input = QSpinBox()
        self.life_input.setRange(0, 999)
        self.life_input.setValue(3)  # 设置生命值默认为1
        life_layout.addWidget(life_label)
        life_layout.addWidget(self.life_input)
        layout.addLayout(life_layout)

        # 攻击力输入
        attack_layout = QHBoxLayout()
        attack_label = QLabel("攻击力:")
        self.attack_input = QSpinBox()
        self.attack_input.setRange(0, 999)
        self.attack_input.setValue(1)  # 设置攻击力默认为1
        attack_layout.addWidget(attack_label)
        attack_layout.addWidget(self.attack_input)
        layout.addLayout(attack_layout)

        # 类型输入
        type_layout = QHBoxLayout()
        type_label = QLabel("类型:")
        self.type_input = QLineEdit()
        type_layout.addWidget(type_label)
        type_layout.addWidget(self.type_input)
        layout.addLayout(type_layout)

    def create_buttons(self, layout):
        button_layout = QHBoxLayout()
        
        # 添加卡片按钮
        add_btn = QPushButton("添加卡片")
        add_btn.clicked.connect(self.add_card)
        button_layout.addWidget(add_btn)
        
        # 删除卡片按钮
        delete_btn = QPushButton("删除卡片")
        delete_btn.setStyleSheet("""
            QPushButton {
                background-color: #ff4444;
            }
            QPushButton:hover {
                background-color: #cc0000;
            }
        """)
        delete_btn.clicked.connect(self.delete_card)
        button_layout.addWidget(delete_btn)
        
        # 生成按钮
        generate_btn = QPushButton("生成JSON")
        generate_btn.clicked.connect(self.generate_json)
        button_layout.addWidget(generate_btn)
        
        # 清空按钮
        clear_btn = QPushButton("清空")
        clear_btn.clicked.connect(self.clear_fields)
        button_layout.addWidget(clear_btn)
        
        layout.addLayout(button_layout)

    def add_card(self):
        # 获取当前输入数据
        card_data = {
            "id": self.id_input.text(),
            "name": self.name_input.text(),
            "symbol": self.symbol_input.text(),
            "description": self.desc_input.toPlainText(),
            "life": self.life_input.value(),
            "attack": self.attack_input.value(),
            "type": self.type_input.text()
        }

        # 验证所有字段
        empty_fields = []
        if not card_data["id"]:
            empty_fields.append("ID")
        if not card_data["name"]:
            empty_fields.append("名称")
        if not card_data["symbol"]:
            empty_fields.append("符号")
        if card_data["life"] == 0:
            empty_fields.append("生命值")
        if card_data["attack"] == 0:
            empty_fields.append("攻击力")
        if not card_data["type"]:
            empty_fields.append("类型")

        if empty_fields:
            QMessageBox.warning(self, "警告", f"以下字段为必填项：\n{', '.join(empty_fields)}")
            return

        # 添加到卡片列表
        self.cards_data.append(card_data)
        self.update_preview_list()
        
        # 清空输入框
        self.clear_fields()
        QMessageBox.information(self, "成功", "卡片已添加到列表！")

    def update_preview_list(self):
        self.preview_list.clear()
        for card in self.cards_data:
            item = QListWidgetItem(f"{card['name']} ({card['id']})")
            item.setData(Qt.UserRole, card)
            self.preview_list.addItem(item)

    def load_card_data(self, item):
        card_data = item.data(Qt.UserRole)
        self.id_input.setText(card_data["id"])
        self.name_input.setText(card_data["name"])
        self.symbol_input.setText(card_data["symbol"])
        self.desc_input.setText(card_data["description"])
        self.life_input.setValue(card_data["life"])
        self.attack_input.setValue(card_data["attack"])
        self.type_input.setText(card_data["type"])

    def generate_json(self):
        if not self.cards_data:
            QMessageBox.warning(self, "警告", "请先添加至少一张卡片！")
            return

        try:
            # 生成JSON文件
            with open("cards_data.json", "w", encoding="utf-8") as f:
                json.dump(self.cards_data, f, ensure_ascii=False, indent=4)
            QMessageBox.information(self, "成功", "JSON文件已生成！")
        except Exception as e:
            QMessageBox.critical(self, "错误", f"生成JSON文件时出错：{str(e)}")

    def clear_fields(self):
        self.id_input.clear()
        self.name_input.clear()
        self.symbol_input.clear()
        self.desc_input.clear()
        self.life_input.setValue(0)
        self.attack_input.setValue(0)
        self.type_input.clear()

    def delete_card(self):
        # 获取当前选中的项
        current_item = self.preview_list.currentItem()
        if not current_item:
            QMessageBox.warning(self, "警告", "请先选择要删除的卡片！")
            return
            
        # 确认删除
        reply = QMessageBox.question(self, "确认删除", 
                                   "确定要删除这张卡片吗？",
                                   QMessageBox.Yes | QMessageBox.No)
        
        if reply == QMessageBox.Yes:
            # 从数据列表中删除
            card_data = current_item.data(Qt.UserRole)
            self.cards_data.remove(card_data)
            # 更新预览列表
            self.update_preview_list()
            # 清空输入框
            self.clear_fields()
            QMessageBox.information(self, "成功", "卡片已删除！")

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = CardInfoWindow()
    window.show()
    sys.exit(app.exec_())
