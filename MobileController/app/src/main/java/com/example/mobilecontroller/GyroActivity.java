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
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import com.example.mobilecontroller.R;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;
import java.net.UnknownHostException;
import java.util.Arrays;
import java.util.List;

import android.widget.AdapterView;


public class GyroActivity extends Activity implements SensorEventListener,View.OnTouchListener {
    private static final String TAG = "GyroUDP";
    private SensorManager sensorManager;
    private Sensor gyroscopeSensor;
    private Sensor rotationVectorSensor;

    private TextView gyroTextView;
    private EditText ipEditText, portEditText;
    private Button connectButton;
    private DatagramSocket udpSocket;
    private InetAddress serverAddress;
    private int serverPort;
    private boolean isConnected = false;
    private Handler mainHandler = new Handler(Looper.getMainLooper());

    private Button buttonUp, buttonDown, buttonA, buttonB;
    // 按键状态映射（避免重复发送）
    private boolean isUpPressed = false;
    private boolean isDownPressed = false;
    private boolean isAPressed = false;
    private boolean isBPressed = false;

    private Spinner playerSpinner; // 新增：玩家选择下拉框
    private int currentPlayer = 1; // 1P 或 2P，默认为 1P

    float gx,gy,gz;     // gyro sensor
    float rx,ry,rz;     // rotation sensor

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_gyro);

        // 初始化视图组件
        gyroTextView = findViewById(R.id.gyro_text_view);
        ipEditText = findViewById(R.id.ip_edit_text);
        portEditText = findViewById(R.id.port_edit_text);
        connectButton = findViewById(R.id.connect_button);

        buttonUp = findViewById(R.id.button_up);
        buttonDown = findViewById(R.id.button_down);
        buttonA = findViewById(R.id.button_a);
        buttonB = findViewById(R.id.button_b);

        // 设置按键触摸监听（处理按下和抬起事件）
        buttonUp.setOnTouchListener(this);
        buttonDown.setOnTouchListener(this);
        buttonA.setOnTouchListener(this);
        buttonB.setOnTouchListener(this);
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

        // 初始化玩家选择下拉框
        playerSpinner = findViewById(R.id.player_spinner);
        playerSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                currentPlayer = position;
            }

            @Override
            public void onNothingSelected(AdapterView<?> parent) {

            }
            // 原有的监听器代码
        });


        // 初始化传感器
        sensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
        gyroscopeSensor = sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE);
        rotationVectorSensor = sensorManager.getDefaultSensor(Sensor.TYPE_ROTATION_VECTOR);

        if (gyroscopeSensor == null) {
            Toast.makeText(this, "设备不支持陀螺仪", Toast.LENGTH_SHORT).show();
            finish();
        }
        if(rotationVectorSensor == null)
        {
            Toast.makeText(this, "设备不支持rotationVector", Toast.LENGTH_SHORT).show();
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
            sensorManager.registerListener(this, rotationVectorSensor, SensorManager.SENSOR_DELAY_NORMAL);

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
            sensorManager.registerListener(this, rotationVectorSensor, SensorManager.SENSOR_DELAY_NORMAL);
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
        boolean hasGyroChanged = false;
        boolean hasRotChanged = false;


        if (event.sensor.getType() == Sensor.TYPE_GYROSCOPE) {
            hasGyroChanged = true;
            gx = event.values[0];
            gy = event.values[1];
            gz = event.values[2];

        }
        else if(event.sensor.getType() == Sensor.TYPE_ROTATION_VECTOR)
        {
            hasRotChanged = true;

            float[] rotationMatrix = new float[9];
            SensorManager.getRotationMatrixFromVector(rotationMatrix, event.values);
            float[] orientation = new float[3];
            SensorManager.getOrientation(rotationMatrix, orientation);
            float yaw = (float) Math.toDegrees(orientation[0]);
            float pitch = (float) Math.toDegrees(orientation[1]);
            float roll = (float) Math.toDegrees(orientation[2]);

            rx = yaw;
            ry = pitch;
            rz = roll;
        }

        if(hasGyroChanged || hasRotChanged)
        {
            // gui
            String gyroInfo = "陀螺仪:\nX: " + gx + " rad/s\nY: " + gy + " rad/s\nZ: " + gz + " rad/s" + "\n";
            String rotInfo = "欧拉角:(" + rx + "," + ry + "," + rz + ")";
            String info = gyroInfo + rotInfo;
            mainHandler.post(() -> gyroTextView.setText(info));

            // 发送UDP数据
            if(isConnected)
            {
                // 格式化数据
                //String data = String.format("GYRO|%.2f|%.2f|%.2f", x, y, z);
                // @miao @test ,temp forbid
                //sendUDPData(data);
            }

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

    @Override
    public boolean onTouch(View v, MotionEvent event) {
        if (!isConnected)
        {
            return false; // 未连接时忽略所有操作
        }

        String command = "";
        int vid = v.getId();
        if(vid == R.id.button_up)
        {
            if (event.getAction() == MotionEvent.ACTION_DOWN) {
                if (!isUpPressed) {
                    command = "UP_PRESS";
                    isUpPressed = true;
                }
            } else if (event.getAction() == MotionEvent.ACTION_UP) {
                command = "UP_RELEASE";
                isUpPressed = false;
            }
        }
        else if(vid == R.id.button_down)
        {
            if (event.getAction() == MotionEvent.ACTION_DOWN) {
                if (!isDownPressed) {
                    command = "DOWN_PRESS";
                    isDownPressed = true;
                }
            } else if (event.getAction() == MotionEvent.ACTION_UP) {
                command = "DOWN_RELEASE";
                isDownPressed = false;
            }
        }
        else if(vid == R.id.button_a)
        {
            if (event.getAction() == MotionEvent.ACTION_DOWN) {
                if (!isAPressed) {
                    command = "A_PRESS";
                    isAPressed = true;
                }
            } else if (event.getAction() == MotionEvent.ACTION_UP) {
                command = "A_RELEASE";
                isAPressed = false;
            }
        }
        else if(vid == R.id.button_b)
        {
            if (event.getAction() == MotionEvent.ACTION_DOWN) {
                if (!isBPressed) {
                    command = "B_PRESS";
                    isBPressed = true;
                }
            } else if (event.getAction() == MotionEvent.ACTION_UP) {
                command = "B_RELEASE";
                isBPressed = false;
            }
        }
        if (!command.isEmpty()) {
            command = "P" + (currentPlayer + 1) + ";" + command;   // player index
            sendUDPData(command); // 发送按键状态指令
            return true; // 消耗事件，避免重复触发
        }
        return false;
    }


}