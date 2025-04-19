using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Import Tilemap namespace

public class ParallaxEffect : MonoBehaviour
{
    private Vector3 startPos; // Initial position of the background
    private Vector2 length; // Width and height of the background
    public GameObject cam; // Reference to the camera
    public float parallaxStrengthX = 0.5f; // Horizontal parallax strength
    public float parallaxStrengthY = 0.5f; // Vertical parallax strength
    public bool infiniteHorizontal = true; // Infinite horizontal scrolling
    public bool infiniteVertical = false; // Infinite vertical scrolling (optional)
    public float smoothSpeed = 5f; // Speed of interpolation for smooth movement

    private Vector3 targetPos; // Target position for smooth movement

    private Renderer rend; // Reference to either SpriteRenderer or TilemapRenderer

    private void Start()
    {
        // Save the initial position of the background
        startPos = transform.position;

        // Try to get the correct renderer (SpriteRenderer or TilemapRenderer)
        rend = GetComponent<SpriteRenderer>() as Renderer;
        if (rend == null) // If no SpriteRenderer, check for TilemapRenderer
        {
            rend = GetComponent<TilemapRenderer>() as Renderer;
        }

        // Check if we successfully got a renderer, and get the size
        if (rend != null)
        {
            length = rend.bounds.size;
        }
        else
        {
            Debug.LogError("No SpriteRenderer or TilemapRenderer found on " + gameObject.name);
        }
    }

    private void LateUpdate()
    {
        // Calculate how much the camera has moved relative to the start position
        float distX = (cam.transform.position.x * parallaxStrengthX);
        float distY = (cam.transform.position.y * parallaxStrengthY);

        // Target position for smooth interpolation
        targetPos = new Vector3(startPos.x + distX, startPos.y + distY, transform.position.z);

        // Smoothly move towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // Infinite Scrolling (optional)
        if (infiniteHorizontal)
        {
            float tempX = cam.transform.position.x * (1 - parallaxStrengthX);
            if (tempX > startPos.x + length.x) startPos.x += length.x;
            else if (tempX < startPos.x - length.x) startPos.x -= length.x;
        }
        if (infiniteVertical)
        {
            float tempY = cam.transform.position.y * (1 - parallaxStrengthY);
            if (tempY > startPos.y + length.y) startPos.y += length.y;
            else if (tempY < startPos.y - length.y) startPos.y -= length.y;
        }
    }
}
