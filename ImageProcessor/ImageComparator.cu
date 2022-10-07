#include <stdio.h>
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <iostream>
#include <time.h>

// GPU�Ōv�Z����ۂ̊֐�
// __global__�̓z�X�g����Ă΂�ăf�o�C�X���Ŏ��s�����
// �߂�l��void�̂݁������Ɍ��ʂ�����ϐ���n���K�v������@������Ăяo�������猩��Ȃ��ƈӖ����Ȃ�����|�C���^�ł���K�v������
// C#��byte�^�����Ȃ�����1bit��C++����unsigned char�ɂȂ�炵��
// ���Ȃ݂�char��signed char��unsigned char�̂ǂ��炩�ɃR���p�C�����U�蕪���邩�璆�g���ǂ��炩�͎����I�Ɍ��܂�炵��
// unsigned char�͕����Ȃ���0�`255�@signed char�͕����t����-128 �` 127
// ���Ȃ݂�signed char��C#����sbyte�ɂȂ�Ƃ��@��������Java���Ƃ��ꂪbyte�炵���@��₱����
__global__ void gpu_function(int* result, unsigned char* b1, unsigned char* b2)
{
	int i = blockIdx.x * blockDim.x + threadIdx.x;
	result[i] = b1[i] + b2[i];
}

// main function
int main(unsigned char* b1Ptr, unsigned char* b2Ptr, int arrayLength)
{
	//�O���b�h�̒��Ƀu���b�N�������Ă��̒��ɃX���b�h������
	// 1�u���b�N���̃X���b�h�� �ő吔��512�炵��
	int threadNum = 512;

	for (int i = 0; i < arrayLength; i++)
	{
		int x = b1Ptr[i];
		std::cout << i << std::endl;
	}

	// �z��̃T�C�Y��z��v�f�̃T�C�Y�Ŋ���Ɣz��̗v�f����������炵��
	// 32bit��int�^��10�������Ƃ�����z��̑傫����320�@�v�f�̃T�C�Y��32�@�����10�ŗv�f�����킩��A��
	//int inputSize = sizeof(b1) / sizeof(b1[0]);
	//int editedSize;

	//std::cout << "in_" << inputSize << std::endl;

	// �����X���b�h���̔{������Ȃ��Ƃ��͒�������
	/*if (inputSize % threadNum != 0)
	{
		editedSize = inputSize % threadNum;
	}

	std::cout << "ed_" << editedSize << std::endl;*/

	//unsigned char* e1 = new unsigned char[editedSize];
	//unsigned char* e2 = new unsigned char[editedSize];

	// �傫���𒲐�������̔z��ɗv�f���R�s�[����
	//for (int i = 0; i <inputSize ; i++)
	//{
	//	e1[i] = b1[i];
	//	e2[i] = b2[i];
	//}

	//// �󂢂Ă��镔����0�Ŗ��߂�
	//for (int i = 0; i < editedSize - inputSize; i++)
	//{
	//	// 0�`inputSize-1�܂ł͊��ɑ���ς݁@�c��̕����ɓ���Ă���
	//	e1[i + inputSize -1] = 0;
	//	e2[i + inputSize -1] = 0;
	//}

	//// ���ʂ����邽�߂̕ϐ�
	//// �z��̑傫���͒萔����Ȃ��ƃ_���炵��
	//int* result = new int[editedSize];

	//// �f�o�C�X���̔z���p��
	//int* d_result = new int[editedSize];
	//unsigned char* d_e1 = new unsigned char[editedSize];
	//unsigned char* d_e2 = new unsigned char[editedSize];

	//// �f�o�C�X�̃��������m��
	//cudaMalloc(&d_result, editedSize * sizeof(int));
	//cudaMalloc(&d_e1, editedSize * sizeof(unsigned char));
	//cudaMalloc(&d_e2, editedSize * sizeof(unsigned char));

	//// �z�X�g���̔z����f�o�C�X���ɃR�s�[
	//cudaMemcpy(d_result, result, editedSize * sizeof(int), cudaMemcpyHostToDevice);
	//cudaMemcpy(d_e1, e1, editedSize * sizeof(unsigned char), cudaMemcpyHostToDevice);
	//cudaMemcpy(d_e2, e2, editedSize * sizeof(unsigned char), cudaMemcpyHostToDevice);

	//// �u���b�N�̑傫�������߂�
	//dim3 block(inputSize / threadNum, 1);
	//// �O���b�h�̑傫�������߂�
	//dim3 grid(1, 1);

	//gpu_function <<<grid, block>>> (d_result, d_e1, d_e2);

	//int r;
	//for (int i = 0; i < editedSize; i++)
	//{
	//	r += result[i];
	//}

	// new������K��delete����A��
	/*delete[] e1;
	delete[] e2;*/
	/*delete[] d_e1;
	delete[] d_e2;
	delete[] result;
	delete[] d_result;*/

	return 0;
	//return r;
}