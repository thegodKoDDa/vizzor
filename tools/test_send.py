"""
Script de teste para envio direto de dados ao visor Rise Mode Aura Ice.
Requer: pip install hidapi (no Windows ou Linux com acesso ao USB)

Uso:
    python test_send.py --cpu 55 --gpu 48 --cpu-load 30
"""

import sys
import time
import argparse

try:
    import hid
except ImportError:
    print("Biblioteca 'hidapi' não encontrada. Instale com: pip install hidapi")
    sys.exit(1)

VENDOR_ID = 0xAA88   # 43656
PRODUCT_ID = 0x8666  # 34406


def clamp(val, min_val=0, max_val=99):
    return max(min_val, min(max_val, int(round(val))))


def build_packet(cpu_temp=0, gpu_temp=0, cpu_load=0, mem_load=0, mb_temp=0, gpu_load=0):
    """
    Constrói o pacote de 11 bytes conforme especificação do protocolo Rise Mode Aura Ice:
    Byte 0: Report ID (0x00)
    Byte 1: Valor no Mostrador Físico / CPU Temp (°C, 0 a 99)
    Byte 2: GPU Temp (°C, 0 a 99)
    Byte 3: CPU Usage (%, 0 a 99)
    Byte 4: RAM Usage (%, 0 a 99)
    Byte 5: Motherboard Temp (°C, 0 a 99)
    Byte 6: GPU Usage (%, 0 a 99)
    Bytes 7-10: Zeros (padding)
    """
    packet = bytearray(11)
    packet[0] = 0x00
    packet[1] = clamp(cpu_temp)
    packet[2] = clamp(gpu_temp)
    packet[3] = clamp(cpu_load)
    packet[4] = clamp(mem_load)
    packet[5] = clamp(mb_temp)
    packet[6] = clamp(gpu_load)
    return bytes(packet)


def main():
    parser = argparse.ArgumentParser(description="Envia métricas de teste para o visor Rise Mode Aura Ice.")
    parser.add_argument("--cpu", type=int, default=50, help="Temperatura da CPU em °C (padrão: 50)")
    parser.add_argument("--gpu", type=int, default=45, help="Temperatura da GPU em °C (padrão: 45)")
    parser.add_argument("--cpu-load", type=int, default=25, help="Uso de CPU em % (padrão: 25)")
    parser.add_argument("--ram-load", type=int, default=50, help="Uso de RAM em % (padrão: 50)")
    parser.add_argument("--mb-temp", type=int, default=35, help="Temperatura da placa-mãe em °C (padrão: 35)")
    parser.add_argument("--gpu-load", type=int, default=10, help="Uso de GPU em % (padrão: 10)")
    parser.add_argument("--interval", type=float, default=1.0, help="Intervalo de repetição em segundos (padrão: 1.0)")
    parser.add_argument("--count", type=int, default=5, help="Quantidade de pacotes a enviar (0 para infinito)")
    parser.add_argument("--duration", type=int, default=0, help="Duração em segundos para manter o envio ativo (0 usa count)")

    args = parser.parse_args()

    print(f"Buscando dispositivo USB HID VID=0x{VENDOR_ID:04X}, PID=0x{PRODUCT_ID:04X}...")
    try:
        device = hid.device()
        device.open(VENDOR_ID, PRODUCT_ID)
        print("Dispositivo conectado com sucesso!")
        print(f"Fabricante: {device.get_manufacturer_string()}")
        print(f"Produto:    {device.get_product_string()}")
        print(f"Serial:     {device.get_serial_number_string()}")
    except Exception as e:
        print(f"Erro ao abrir dispositivo: {e}")
        print("Verifique se o software oficial da Rise Mode está fechado e o serviço 'tifSer' parado.")
        return

    try:
        packet = build_packet(
            cpu_temp=args.cpu,
            gpu_temp=args.gpu,
            cpu_load=args.cpu_load,
            mem_load=args.ram_load,
            mb_temp=args.mb_temp,
            gpu_load=args.gpu_load
        )
        print(f"Enviando pacote ({len(packet)} bytes): {list(packet)}")

        start_time = time.time()
        sent = 0
        while True:
            bytes_written = device.write(packet)
            sent += 1
            elapsed = time.time() - start_time
            print(f"[{sent}] Enviados {bytes_written} bytes ({elapsed:.1f}s decorridos)...", end="\r")

            if args.duration > 0 and elapsed >= args.duration:
                print(f"\nDuração de {args.duration}s atingida.")
                break
            if args.duration == 0 and args.count > 0 and sent >= args.count:
                print(f"\nTotal de {args.count} pacotes atingido.")
                break

            time.sleep(args.interval)

    except KeyboardInterrupt:
        print("\nInterrompido pelo usuário.")
    finally:
        device.close()
        print("Conexão fechada.")


if __name__ == "__main__":
    main()
