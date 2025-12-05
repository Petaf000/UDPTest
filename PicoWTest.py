import asyncio
import struct
from bleak import BleakScanner, BleakClient

# ★Pico Wのアドレス (さっき成功したときのアドレスのまま)
TARGET_ADDRESS = "88:A2:9E:02:3E:F4" 

UART_TX_CHAR_UUID = "6E400003-B5A3-F393-E0A9-E50E24DCCA9E"

# データを一時的に溜めておくバッファ
buffer = bytearray()

def parse_rvc_packet(packet):
    """
    RVCパケット(19バイト)を解析して表示する
    Format: Header(2)|Index(1)|Yaw(2)|Pitch(2)|Roll(2)|X_acc(2)|Y_acc(2)|Z_acc(2)|...
    """
    try:
        # ヘッダー(AAAA)とIndexを除くデータの開始位置
        # struct.unpack('<hhhhhh', ...) : リトルエンディアンのshort(2byte) x 6個
        # Yaw, Pitch, Roll, AccX, AccY, AccZ
        values = struct.unpack('<hhhhhh', packet[3:15])
        
        yaw = values[0] * 0.01
        pitch = values[1] * 0.01
        roll = values[2] * 0.01
        
        # 見やすいように整形して表示
        print(f"Yaw: {yaw:6.2f}° | Pitch: {pitch:6.2f}° | Roll: {roll:6.2f}°")
        
    except Exception as e:
        print(f"Parse Error: {e}")

def notification_handler(sender, data):
    global buffer
    buffer.extend(data)
    
    # バッファの中から 0xAAAA (ヘッダー) を探して切り出す
    while len(buffer) >= 19:
        # ヘッダーを探す
        header_index = buffer.find(b'\xaa\xaa')
        
        if header_index == -1:
            # ヘッダーが見つからないなら、最後の1バイトを残して捨てる（次の結合待ち）
            buffer = buffer[-1:]
            break
            
        # ヘッダーが先頭に来るように捨てる
        if header_index > 0:
            buffer = buffer[header_index:]
            
        # 1パケット分のデータが溜まっているか確認
        if len(buffer) >= 19:
            packet = buffer[:19]
            parse_rvc_packet(packet)
            
            # 解析した分をバッファから消す
            buffer = buffer[19:]

async def main():
    print(f"デバイス {TARGET_ADDRESS} に接続中...")
    
    device = await BleakScanner.find_device_by_address(TARGET_ADDRESS, timeout=20.0)
    if not device:
        print("見つかりませんでした。Pico Wを再起動してください。")
        return

    async with BleakClient(device) as client:
        print("接続成功！センサーを動かしてみてください。")
        await client.start_notify(UART_TX_CHAR_UUID, notification_handler)
        
        # ずっと受信し続ける
        while True:
            await asyncio.sleep(1)

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n終了")