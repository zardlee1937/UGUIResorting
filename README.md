# UGUI Resorting

## 功能

* 保存UGUI层级变换数据到xml
* 由xml读取数据恢复UGUI层级变换数据，并根据分辨率适配尺寸

## 用法

1. 以如下结构制作UI</br>
    -canvas</br>
        |-root</br>
            |-component_node1</br>
            |-component_node2</br>
                    ...</br>
            |-component_node3</br>
                    |-component_node4</br>
                            ...</br>
2. 在root上加入UGUIBaker脚本，点击Bake，数据会保存在StreamingAssets/GUI下
3. 在root上加入UGUIReset脚本，将xml包含到工程资源中，拖拽到data上，输入当前分辨率，点击Reset
