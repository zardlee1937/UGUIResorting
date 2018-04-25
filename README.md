#UGUI Resorting

## 功能

* 保存UGUI层级变换数据到xml
* 由xml读取数据恢复UGUI层级变换数据，并根据分辨率适配尺寸

## 用法

1. 以如下结构制作UI
    -canvas
        |-root
            |-component_node1
            |-component_node2
                    ...
            |-component_node3
                    |-component_node4
                            ...
2. 在root上加入UGUIBaker脚本，点击Bake，数据会保存在StreamingAssets/GUI下
3. 在root上加入UGUIReset脚本，将xml包含到工程资源中，拖拽到data上，输入当前分辨率，点击Reset