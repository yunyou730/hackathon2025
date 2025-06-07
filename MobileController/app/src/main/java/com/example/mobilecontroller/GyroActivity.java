package com.example.mobilecontroller;

import android.app.Activity;
import android.content.Context;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.example.mobilecontroller.R;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;
import java.net.UnknownHostException;

public class GyroActivity extends Activity implements SensorEventListener {
    private static final String TAG = "GyroUDP";
    private SensorManager sensorManager;
    private Sensor gyroscopeSensor;
    private TextView gyroTextView;
    private EditText ipEditText, portEditText;
    private Button connectButton;
    private DatagramSocket udpSocket;
    private InetAddress serverAddress;
    private int serverPort;
    private boolean isConnected = false;
    private Handler mainHandler = new Handler(Looper.getMainLooper());

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_gyro);

        // 初始化视图组件
        gyroTextView = findViewById(R.id.gyro_text_view);
        ipEditText = findViewById(R.id.ip_edit_text);
        portEditText = findViewById(R.id.port_edit_text);
        connectButton = findViewById(R.id.connect_button);

        // 设置连接按钮点击事件
        connectButton.setOnClickListener(v -> {
            String ip = ipEditText.getText().toString().trim();
            String portStr = portEditText.getText().toString().trim();

            if (ip.isEmpty() || portStr.isEmpty()) {
                Toast.makeText(GyroActivity.this, "请填写完整的IP和端口", Toast.LENGTH_SHORT).show();
                return;
            }

            try {
                serverPort = Integer.parseInt(portStr);
                serverAddress = InetAddress.getByName(ip);
                setupUDPSocket();
                toggleConnectionState();
            } catch (NumberFormatException | UnknownHostException e) {
                Toast.makeText(GyroActivity.this, "地址或端口格式错误", Toast.LENGTH_SHORT).show();
                e.printStackTrace();
            }
        });

        // 初始化传感器
        sensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
        gyroscopeSensor = sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE);

        if (gyroscopeSensor == null) {
            Toast.makeText(this, "设备不支持陀螺仪", Toast.LENGTH_SHORT).show();
            finish();
        }
    }

    private void setupUDPSocket() {
        try {
            udpSocket = new DatagramSocket();
        } catch (SocketException e) {
            Log.e(TAG, "创建UDP Socket失败: " + e.getMessage());
            Toast.makeText(this, "创建Socket失败", Toast.LENGTH_SHORT).show();
        }
    }

    private void toggleConnectionState() {
        isConnected = !isConnected;
        connectButton.setText(isConnected ? "断开连接" : "连接");

        if (isConnected) {
            sensorManager.registerListener(this, gyroscopeSensor, SensorManager.SENSOR_DELAY_GAME);
        } else {
            sensorManager.unregisterListener(this);
            if (udpSocket != null) {
                udpSocket.close();
                udpSocket = null;
            }
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (isConnected) {
            sensorManager.registerListener(this, gyroscopeSensor, SensorManager.SENSOR_DELAY_GAME);
        }
    }

    @Override
    protected void onPause() {
        super.onPause();
        sensorManager.unregisterListener(this);
        if (isConnected && udpSocket != null) {
            udpSocket.close();
            isConnected = false;
            connectButton.setText("连接");
        }
    }

    @Override
    public void onSensorChanged(SensorEvent event) {
        if (event.sensor.getType() == Sensor.TYPE_GYROSCOPE && isConnected) {
            float x = event.values[0];
            float y = event.values[1];
            float z = event.values[2];

            // 格式化数据
            String data = String.format("GYRO|%.2f|%.2f|%.2f", x, y, z);

            // 更新UI
            mainHandler.post(() -> gyroTextView.setText("陀螺仪数据:\nX: " + x + " rad/s\nY: " + y + " rad/s\nZ: " + z + " rad/s"));

            // 发送UDP数据
            sendUDPData(data);
        }
    }

    private void sendUDPData(String data) {
        if (udpSocket != null && serverAddress != null) {
            new Thread(() -> {
                try {
                    byte[] buffer = data.getBytes();
                    DatagramPacket packet = new DatagramPacket(buffer, buffer.length, serverAddress, serverPort);
                    udpSocket.send(packet);
                } catch (IOException e) {
                    Log.e(TAG, "发送UDP数据失败: " + e.getMessage());
                    runOnUiThread(() -> Toast.makeText(GyroActivity.this, "数据发送失败", Toast.LENGTH_SHORT).show());
                }
            }).start();
        }
    }

    @Override
    public void onAccuracyChanged(Sensor sensor, int accuracy) {
        // 不需要处理
    }
}