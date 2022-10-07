#include <stdio.h>
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <iostream>
#include <time.h>

// GPUで計算する際の関数
// __global__はホストから呼ばれてデバイス側で実行される
// 戻り値はvoidのみ→引数に結果を入れる変数を渡す必要がある　それを呼び出し側から見れないと意味がないからポインタである必要がある
// C#のbyte型符号なし整数1bitはC++だとunsigned charになるらしい
// ちなみにcharはsigned charかunsigned charのどちらかにコンパイラが振り分けるから中身がどちらかは自動的に決まるらしい
// unsigned charは符号なし→0～255　signed charは符号付き→-128 ～ 127
// ちなみにsigned charはC#だとsbyteになるとか　豆だけどJavaだとこれがbyteらしい　ややこしい
__global__ void gpu_function(int* result, unsigned char* b1, unsigned char* b2)
{
	int i = blockIdx.x * blockDim.x + threadIdx.x;
	result[i] = b1[i] + b2[i];
}

// main function
int main(unsigned char* b1Ptr, unsigned char* b2Ptr, int arrayLength)
{
	//グリッドの中にブロックがあってその中にスレッドがある
	// 1ブロック内のスレッド数 最大数が512らしい
	int threadNum = 512;

	for (int i = 0; i < arrayLength; i++)
	{
		int x = b1Ptr[i];
		std::cout << i << std::endl;
	}

	// 配列のサイズを配列要素のサイズで割ると配列の要素数が分かるらしい
	// 32bitのint型が10個あったとしたら配列の大きさが320　要素のサイズが32　割ると10で要素数がわかる、と
	//int inputSize = sizeof(b1) / sizeof(b1[0]);
	//int editedSize;

	//std::cout << "in_" << inputSize << std::endl;

	// 数がスレッド数の倍数じゃないときは調整する
	/*if (inputSize % threadNum != 0)
	{
		editedSize = inputSize % threadNum;
	}

	std::cout << "ed_" << editedSize << std::endl;*/

	//unsigned char* e1 = new unsigned char[editedSize];
	//unsigned char* e2 = new unsigned char[editedSize];

	// 大きさを調整した後の配列に要素をコピーする
	//for (int i = 0; i <inputSize ; i++)
	//{
	//	e1[i] = b1[i];
	//	e2[i] = b2[i];
	//}

	//// 空いている部分を0で埋める
	//for (int i = 0; i < editedSize - inputSize; i++)
	//{
	//	// 0～inputSize-1までは既に代入済み　残りの部分に入れていく
	//	e1[i + inputSize -1] = 0;
	//	e2[i + inputSize -1] = 0;
	//}

	//// 結果を入れるための変数
	//// 配列の大きさは定数じゃないとダメらしい
	//int* result = new int[editedSize];

	//// デバイス側の配列を用意
	//int* d_result = new int[editedSize];
	//unsigned char* d_e1 = new unsigned char[editedSize];
	//unsigned char* d_e2 = new unsigned char[editedSize];

	//// デバイスのメモリを確保
	//cudaMalloc(&d_result, editedSize * sizeof(int));
	//cudaMalloc(&d_e1, editedSize * sizeof(unsigned char));
	//cudaMalloc(&d_e2, editedSize * sizeof(unsigned char));

	//// ホスト側の配列をデバイス側にコピー
	//cudaMemcpy(d_result, result, editedSize * sizeof(int), cudaMemcpyHostToDevice);
	//cudaMemcpy(d_e1, e1, editedSize * sizeof(unsigned char), cudaMemcpyHostToDevice);
	//cudaMemcpy(d_e2, e2, editedSize * sizeof(unsigned char), cudaMemcpyHostToDevice);

	//// ブロックの大きさを決める
	//dim3 block(inputSize / threadNum, 1);
	//// グリッドの大きさを決める
	//dim3 grid(1, 1);

	//gpu_function <<<grid, block>>> (d_result, d_e1, d_e2);

	//int r;
	//for (int i = 0; i < editedSize; i++)
	//{
	//	r += result[i];
	//}

	// newしたら必ずdeleteする、と
	/*delete[] e1;
	delete[] e2;*/
	/*delete[] d_e1;
	delete[] d_e2;
	delete[] result;
	delete[] d_result;*/

	return 0;
	//return r;
}