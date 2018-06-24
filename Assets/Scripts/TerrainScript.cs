using UnityEngine;
using System;
using System.Collections;


public class TerrainScript : MonoBehaviour {

    // компарер для удобной сортировки массива меша
    public class Vector3Comparer : IComparer
    {
        public int Compare(object a, object b)
        {
            Vector3 c = (Vector3) a;
            Vector3 d = (Vector3) b;
            if (c.x > d.x) return 1;
            if (c.x < d.x) return -1; else return 0;
        }
    }

    Mesh mesh;
    Vector3[] terrMap;
	public int holeSliceCount;
    public float maxY;
    public float minY;
    public float terrain_texture_scale;

    // Use this for initialization
    void Start () {

        // карта высот меша поверхности
        terrMap = (Vector3[]) MakeTerrMap().ToArray(typeof(Vector3));

        // поиск самой высокой точки меша
        maxY = terrMap[0].y; 
        for (int i = 0; i<terrMap.Length; i++)
        if (terrMap[i].y > maxY) maxY = terrMap[i].y;

        minY = 0; // самая низкая точка меша

        // пересчитываем поверхность
        RecalculateTerr();
    }

    // строим карту поверхности из картинки
    private ArrayList MakeTerrMap ()
    {
        ArrayList terrMapList = new ArrayList();
        Texture2D txtr = (Texture2D) GetComponent<MeshRenderer>().material.mainTexture;

        // режем исходную картинку на дискретные слайсы и измеряем высоту поверхности в каждом
        // границу поверхности определяем по альфа-каналу точки картинки. Непрозрачное - земля, прозрачное - небо
        for (int i = 0; i <= txtr.width; i+= holeSliceCount* (int)terrain_texture_scale)
        {
            int j = txtr.height;
            while (j-- > 0 && txtr.GetPixel(i, j).a == 0) {  }
            terrMapList.Add(new Vector3((float)i/ terrain_texture_scale, (float)j / terrain_texture_scale));
        }
        return terrMapList;
    }

    // строим меш поверхности
    void RecalculateTerr ()
	{
        mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        int vertLenght = terrMap.Length * 2 + 1; //количество вершин

        Vector3[] vertices = new Vector3[vertLenght];
        int[] tri = new int[vertLenght * 6 - 3]; //количество треугольников

        //цикл добавления вершин
        int j = 1;
        for (int i = 0; i < terrMap.Length; i++)
        {
            vertices[j] = terrMap[i];
            vertices[j + 1] = terrMap[i];
            vertices[j + 1].y = -5;
            j += 2;
        }

        //цикл добавления треугольников
        for (int i = 0; i < terrMap.Length * 2 - 1; i++)
        {
            j = i * 3;
            tri[j] = i + i % 2;
            tri[j + 1] = i + 1 - i % 2;
            tri[j + 2] = i + 2;
        }

        Vector2[] uv = new Vector2[vertLenght];
        uv[0] = new Vector2(0, 0);

		float x;
		float y;
		float dx = vertices[vertices.Length-1].x - vertices[1].x;
		float dy = maxY - minY;

        //цикл добавления текстурных координат
        for (int i = 1; i < terrMap.Length*2; i+=2) {
		x = (vertices[i].x - vertices[1].x)/dx;
		y = (vertices[i].y - minY)/dy;
		uv[i] = new Vector2(x,y);
		uv[i+1] = new Vector2(x,0);
		}

        // строим меш
        mesh.vertices = vertices;
        mesh.triangles = tri;
        mesh.uv = uv;

        // заодно перестраиваем коллайдер меша
        TerrColliderHole();
    }

    // пересчитываем координаты поверхности для участка, затронутого взрывом
    // для этого берем координаты и диаметр взрыва, режем получившийся участок 
    // на вертикальные слайсы с заранее заданным шагом
    // и определяем нужную высоту поверхности после взрыва
    // с учетом осыпания земли, не попавшей внутрь области взрыва
    public void TerrainHole (GameObject explosion, float explDiam)//Vector3[] terrMap)
    {
        Transform expTransf = explosion.GetComponent<Transform>();

		bool intersection = false;
		int rightCorner = 0;
		int leftCorner = 0;
		
        int i = 0;
        int firstDel = 0;
        float x1 = terrMap[0].x, x2 = terrMap[1].x;
        // считаем координаты первого слайса
        float sliceX = expTransf.position.x - explDiam / 2 - explDiam / holeSliceCount;
		float maxX = terrMap[terrMap.Length-1].x;
        ArrayList holeMap = new ArrayList();

        // вычисляем высоту поверхности после взрыва в каждом слайсе
        for (int j = 0; (j <= holeSliceCount); j++) { //выполняем, пока не переберем все слайсы взрыва
            
			sliceX += explDiam / holeSliceCount;

            // нужно для корректного вычисления высот при взрыве с правого края поверхности
            if (sliceX > maxX) 
			{
				sliceX = maxX;
				j = holeSliceCount;
				rightCorner--;
			}

            // нужно для корректного вычисления высот при взрыве с левого края поверхности
            if (sliceX < terrMap[0].x) {
				leftCorner = 1;
				sliceX = terrMap[0].x;
				continue;
			}

            // вычисляем, какой отрезок поверхности будет обрабатываться в текущем слайсе
            while (!((x1 <= sliceX) & (x2 >= sliceX)))
				{
                    i++;
                    x1 = terrMap[i].x;
                    x2 = terrMap[i + 1].x;
                }

            //  считаем высоту поверхности
            float ty1 = terrMap[i].y, ty2 = terrMap[i + 1].y;
            float terrY = ty2 - (ty2 - ty1) * (x2 - sliceX) / (x2 - x1); //уравнение отрезка
            float sqrt = (float)Math.Sqrt(Math.Abs(Math.Pow(explDiam / 2, 2) - Math.Pow(sliceX - expTransf.position.x, 2)));
            float sliceY;

            if (terrY > expTransf.position.y + sqrt) sliceY = terrY - 2 * sqrt;
            else sliceY = expTransf.position.y - sqrt; //уравнение окружности

			if (j == 0) firstDel = i; //первый элемент для удаления. Последний - i

            //не делаем дырку, если взрыв над поверхностью
            if (terrY > sliceY) {	
				holeMap.Add(new Vector3(sliceX, sliceY));
				intersection = true; 
			}
			else holeMap.Add(new Vector3(sliceX, terrY));

        }

        // заменяем пересчитанный участок в карте поверхности
        if (intersection)
        {
			Vector3[] rejectedMap = new Vector3[terrMap.Length - i + firstDel + rightCorner - leftCorner];
            Array.Copy(terrMap, 0, rejectedMap, 0, firstDel + 1 - leftCorner);	
            Array.Copy(terrMap, i + 1, rejectedMap, firstDel + 1 - leftCorner, terrMap.Length - i - 1 + rightCorner);//
		
			foreach (Vector3 v in rejectedMap);
            //Добавляем в мап
            Vector3[] holeMapArr = (Vector3[])holeMap.ToArray(typeof(Vector3));
            terrMap = new Vector3[rejectedMap.Length + holeMapArr.Length];
            Array.Copy(rejectedMap, terrMap, rejectedMap.Length);
            Array.Copy(holeMapArr, 0, terrMap, rejectedMap.Length, holeMapArr.Length);

            //сортируем результат
            IComparer vComparer = new Vector3Comparer();
            Array.Sort(terrMap, vComparer);

            //делаем дырку в коллайдере
            RecalculateTerr();			
        }
    }

    // пересчитываем коллайдер поверхности
    void TerrColliderHole ()
    {
        EdgeCollider2D collider = GetComponent<EdgeCollider2D>();
        Vector2[] terrMap2d = new Vector2[terrMap.Length];
        for (int i = 0; i < terrMap.Length; i++) terrMap2d[i] = terrMap[i];
        collider.points = terrMap2d;
    }

    // Update is called once per frame
    void Update () {

    }

}
