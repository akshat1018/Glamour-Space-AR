using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;



/// <summary>
/// youtube : @SimpleUnity
/// </summary>
public class scroll_snap : MonoBehaviour
{
    public ScrollRect sr;
    public GameObject dots_p;
    public GameObject dot_prefab;
    public Sprite in_sp, out_sp;
    public int start_index;


    GameObject scrollbar;
    GameObject content;

    float[] pos;
    int in_drag;
    float mouse_down_time;
    bool ok_to_lerp;
    float on_down_value;
    float last_value = 0;

    float distance;
    bool is_mouse_down;

    int UILayer;
    int frames_to_10;
    // Start is called before the first frame update
    public void Awake()
    {

        Application.targetFrameRate = 60;

    }
    void Start()
    {
        UILayer = LayerMask.NameToLayer("btn");
        content = sr.content.gameObject;
        if (sr.horizontal)
        {
            scrollbar = sr.horizontalScrollbar.gameObject;
        }else
        {
            scrollbar = sr.verticalScrollbar.gameObject;
        }
        ok_to_lerp = false;
        is_mouse_down = false;

        //adding guid dots
        for (int i = 0; i < content.transform.childCount; i++)
        {
            GameObject g = Instantiate(dot_prefab, dots_p.transform);
            g.GetComponent<Image>().sprite = out_sp;
        }

        // getting position for each child in content of scroll view
        pos = new float[content.transform.childCount];
        distance = 1f / (pos.Length - 1f);
        int ii = pos.Length - 1;
        if (sr.horizontal)
        {
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = distance * i;
                ii--;
            }
        }
        else
        {
            for (int i = 0; i < pos.Length; i++)
            {
                pos[i] = distance * ii;
                ii--;
            }
        }

        //set up are start index
        if (start_index >= 0 && start_index < pos.Length)
        {
            in_drag = start_index;
        }
        
        // in_drag is a index of pos list that we always lerp the content to it

        scrollbar.GetComponent<Scrollbar>().value = pos[in_drag];
        check_dots_sp(in_drag);
        frames_to_10 = 0;
        ok_to_lerp = true;

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && IsPointerOverUIElement() == false)
        {
            pointer_down();
        }
        if (is_mouse_down)
        {
            mouse_down_time += Time.deltaTime;
            last_value = scrollbar.GetComponent<Scrollbar>().value;
        }
        if (Input.GetMouseButtonUp(0) && is_mouse_down)
        {
            pointer_up();
        }
        if (ok_to_lerp && frames_to_10 > 10)
        {
            scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[in_drag], 0.1f);
            check_dots_sp(in_drag);
        }
        for (int i = 0; i < pos.Length; i++)
        {
            last_value = scrollbar.GetComponent<Scrollbar>().value;
            if (last_value < pos[i] + (distance / 2) && last_value > pos[i] - (distance / 2))
            {
                content.transform.GetChild(i).localScale = Vector2.Lerp(content.transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);

                for (int j = 0; j < pos.Length; j++)
                {
                    if (j != i)
                    {
                        content.transform.GetChild(j).localScale = Vector2.Lerp(content.transform.GetChild(j).localScale, new Vector2(0.7f, 0.7f), 0.1f);
                    }
                }
            }
        }
        if(frames_to_10 < 11)
        {
            frames_to_10++;
        }
    }
    public void check_dots_sp(int i)
    {
        for (int b = 0; b < dots_p.transform.childCount; b++)
        {
            if (i == b)
            {
                dots_p.transform.GetChild(b).GetComponent<Image>().sprite = in_sp;
            }
            else
            {
                dots_p.transform.GetChild(b).GetComponent<Image>().sprite = out_sp;
            }
        }
    }
    public void next()
    {
        if (in_drag < pos.Length - 1)
        {
            in_drag++;
        }

    }
    public void back()
    {
        if (in_drag >= 1)
        {
            in_drag--;
        }
    }


    public void pointer_down()
    {
        on_down_value = scrollbar.GetComponent<Scrollbar>().value;
        ok_to_lerp = false;
        mouse_down_time = 0;
        is_mouse_down = true;
    }
    public void pointer_up()
    {
        for (int i = 0; i < pos.Length; i++)
        {
            if (last_value < pos[i] + (distance / 2) && last_value > pos[i] - (distance / 2))
            {
                if (in_drag == i)
                {
                    if (mouse_down_time <= 0.5f)
                    {
                        if (on_down_value < scrollbar.GetComponent<Scrollbar>().value && scrollbar.GetComponent<Scrollbar>().value - on_down_value > 0.03f)
                        {

                            if (sr.horizontal)
                            {
                                if (in_drag < (pos.Length - 1))
                                {
                                    in_drag++;
                                }

                            }
                            else
                            {
                                if (in_drag > 0)
                                {
                                    in_drag--;
                                }
                            }

                            ok_to_lerp = true;
                        }
                        else if (on_down_value > scrollbar.GetComponent<Scrollbar>().value && on_down_value - scrollbar.GetComponent<Scrollbar>().value > 0.03f)
                        {
                            if (sr.horizontal)
                            {
                                if (in_drag > 0)
                                {
                                    in_drag--;
                                }

                            }
                            else
                            {
                                if (in_drag < (pos.Length - 1))
                                {
                                    in_drag++;
                                }
                            }
                            ok_to_lerp = true;
                        }
                        else
                        {
                            ok_to_lerp = true;
                        }
                    }
                    else
                    {
                        ok_to_lerp = true;
                    }
                }
                else
                {
                    in_drag = i;
                    ok_to_lerp = true;
                }
            }
        }
        is_mouse_down = false;
    }





    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }


    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }


    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }


}