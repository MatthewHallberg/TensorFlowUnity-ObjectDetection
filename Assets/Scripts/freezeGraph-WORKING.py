import tensorflow as tf
import os
from tensorflow.python.tools import freeze_graph

# From tensorflow/models/research/  run this:
#protoc object_detection/protos/*.proto --python_out=.
# From tensorflow/models/research/
#export PYTHONPATH=$PYTHONPATH:`pwd`:`pwd`/slim
#MAKE SURE TO DOWNLOAD TENSORFLOW 1.4 with pip install tensorflow==1.4
print tf.VERSION
sess=tf.Session()    
#First let's load meta graph and restore weights
saver = tf.train.import_meta_graph('ssd_mobilenet_v1_coco_2017_11_17/model.ckpt.meta')
saver.restore(sess,tf.train.latest_checkpoint('ssd_mobilenet_v1_coco_2017_11_17/'))
tf.train.write_graph(sess.graph_def, 'ssd_mobilenet_v1_coco_2017_11_17/', 'mobileGRAPH2.pbtxt')

freeze_graph.freeze_graph('ssd_mobilenet_v1_coco_2017_11_17/mobileGRAPH2.pbtxt',
              input_binary = False,
              input_checkpoint = 'ssd_mobilenet_v1_coco_2017_11_17/model.ckpt',
              output_node_names = "num_detections,detection_boxes,detection_scores,detection_classes",
              #output_node_names = "num_detections,detection_boxes,detection_scores,detection_classes",
              output_graph = 'ssd_mobilenet_v1_coco_2017_11_17/newbytes1.4.bytes',
              clear_devices = True, initializer_nodes = "",input_saver = "",
              restore_op_name = "save/restore_all", filename_tensor_name = "save/Const:0")


